using Legado.Core;
using Legado.Core.Data;
using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using Legado.Core.Helps.Books;
using Legado.Core.Models.WebBooks;
using Legado.Core.Mvvm;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Shared
{
    [SingletonDependency]
    public class LegadoContext : QBindableAppBase, IDisposable
    {
        private readonly IEventProvider _eventProvider;
        private readonly AppDatabase _appDb;
        private INativeAudioService _audioService;
        private Timer _progressTimer;

        #region 书籍相关属性

        private List<BookSource> _bookSources = new List<BookSource>();
        /// <summary>
        /// 书源列表
        /// </summary>
        public List<BookSource> BookSources
        {
            get => _bookSources;
            private set => SetProperty(ref _bookSources, value);
        }

        private WebBook _webBook = new WebBook();
        /// <summary>
        /// WebBook实例
        /// </summary>
        public WebBook WebBook
        {
            get => _webBook;
            private set => SetProperty(ref _webBook, value);
        }

        private int _bookIndex = 6;
        /// <summary>
        /// 当前书源索引
        /// </summary>
        public int BookIndex
        {
            get => _bookIndex;
            set => SetProperty(ref _bookIndex, value);
        }

        private Book _currentBook;
        /// <summary>
        /// 当前书籍
        /// </summary>
        public Book CurrentBook
        {
            get => _currentBook;
            set
            {
                if (SetProperty(ref _currentBook, value))
                {
                    CurrentChapter = null;
                    CurrentChapters = null;
                }
            }
        }

        private List<BookChapter> _currentChapters;
        /// <summary>
        /// 当前章节列表
        /// </summary>
        public List<BookChapter> CurrentChapters
        {
            get => _currentChapters;
            private set => SetProperty(ref _currentChapters, value);
        }

        private BookChapter _currentChapter;
        /// <summary>
        /// 当前章节
        /// </summary>
        public BookChapter CurrentChapter
        {
            get => _currentChapter;
            set => SetProperty(ref _currentChapter, value);
        }

        /// <summary>
        /// 当前书源
        /// </summary>
        public BookSource CurrentBookSource => BookSources[BookIndex];

        #endregion

        #region 音频播放状态

        private bool _isPlaying = false;
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        private long _readPosition = 0;
        /// <summary>
        /// 当前读文本位置
        /// </summary>
        public long ReadPosition
        {
            get => _readPosition;
            set => SetProperty(ref _readPosition, value);
        }

        private long _readTotal = 0;
        /// <summary>
        /// 总字数
        /// </summary>
        public long ReadTotal
        {
            get => _readTotal;
            set => SetProperty(ref _readTotal, value);
        }

        private int _volume = 70;
        /// <summary>
        /// 音量 (0-100)
        /// </summary>
        public int Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        private bool _isMuted = false;
        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        private int _previousVolume = 70;
        /// <summary>
        /// 静音前的音量
        /// </summary>
        public int PreviousVolume
        {
            get => _previousVolume;
            set => SetProperty(ref _previousVolume, value);
        }

        private double _playbackSpeed = 1.0;
        /// <summary>
        /// 播放速度
        /// </summary>
        public double PlaybackSpeed
        {
            get => _playbackSpeed;
            set => SetProperty(ref _playbackSpeed, value);
        }

        private bool _autoPlayNext = true;
        /// <summary>
        /// 自动播放下一章
        /// </summary>
        public bool AutoPlayNext
        {
            get => _autoPlayNext;
            set => SetProperty(ref _autoPlayNext, value);
        }

        private bool _rememberPosition = true;
        /// <summary>
        /// 记住播放位置
        /// </summary>
        public bool RememberPosition
        {
            get => _rememberPosition;
            set => SetProperty(ref _rememberPosition, value);
        }

        #endregion

        public LegadoContext(IEventProvider eventProvider, AppDatabase appDatabase)
        {
            _eventProvider = eventProvider;
            _appDb = appDatabase;
        }

        #region 音频服务管理

        /// <summary>
        /// 初始化音频服务
        /// </summary>
        public void InitializeAudioService(INativeAudioService audioService)
        {
            if (_audioService != null)
            {
                // 取消之前的订阅
                _audioService.IsPlayingChanged -= OnIsPlayingChanged;
                _audioService.PlayFinished -= OnPlayFinished;
                _audioService.PlayFailed -= OnPlayFailed;
            }

            _audioService = audioService;

            if (_audioService != null)
            {
                // 订阅音频服务事件
                _audioService.IsPlayingChanged += OnIsPlayingChanged;
                _audioService.PlayFinished += OnPlayFinished;
                _audioService.PlayFailed += OnPlayFailed;

                // 启动进度更新定时器
                _progressTimer?.Dispose();
                _progressTimer = new Timer(_ => UpdateProgress(), null, 333, 333);
            }
        }

        /// <summary>
        /// 音频服务实例
        /// </summary>
        public INativeAudioService AudioService => _audioService;

        /// <summary>
        /// 更新播放进度
        /// </summary>
        private void UpdateProgress()
        {
            if (_audioService != null && IsPlaying)
            {
                ReadPosition = (long)_audioService.CurrentPositionMillisecond;
                ReadTotal = (long)_audioService.CurrentDurationMillisecond;
            }
        }

        /// <summary>
        /// 播放状态变化事件
        /// </summary>
        private void OnIsPlayingChanged(object sender, bool isPlaying)
        {
            IsPlaying = isPlaying;
        }

        /// <summary>
        /// 播放完成事件
        /// </summary>
        private async void OnPlayFinished(object sender, EventArgs e)
        {
            IsPlaying = false;
            if (AutoPlayNext)
            {
                await NextChapterAsync();
            }
        }

        /// <summary>
        /// 播放失败事件
        /// </summary>
        private void OnPlayFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Audio playback failed");
            IsPlaying = false;
        }

        /// <summary>
        /// 播放/暂停
        /// </summary>
        public async Task TogglePlayAsync()
        {
            if (_audioService == null) return;

            if (IsPlaying)
            {
                await _audioService.PauseAsync();
                await SaveToShelfAsync();
            }
            else
            {
                await _audioService.PlayAsync(_audioService.CurrentPositionMillisecond);
            }
        }

        /// <summary>
        /// 设置播放进度
        /// </summary>
        public async Task SeekAsync(long ms)
        {
            if (_audioService == null) return;
            this.ReadPosition = ms;
            await _audioService.SetCurrentTime(ms);
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        public async Task SetVolumeAsync(int value)
        {
            if (_audioService == null) return;
            Volume = value;
            await _audioService.SetVolume(value);

            if (value > 0 && IsMuted)
            {
                IsMuted = false;
                await _audioService.SetMuted(false);
            }
            else if (value == 0 && !IsMuted)
            {
                IsMuted = true;
                await _audioService.SetMuted(true);
            }
        }

        /// <summary>
        /// 切换静音
        /// </summary>
        public async Task ToggleMuteAsync()
        {
            if (_audioService == null) return;

            if (IsMuted)
            {
                Volume = PreviousVolume;
                IsMuted = false;
                await _audioService.SetVolume(Volume);
                await _audioService.SetMuted(false);
            }
            else
            {
                PreviousVolume = Volume;
                Volume = 0;
                IsMuted = true;
                await _audioService.SetVolume(0);
                await _audioService.SetMuted(true);
            }
        }

        /// <summary>
        /// 初始化并播放章节
        /// </summary>
        public async Task PlayChapterAsync(string audioUrl, int positionMs = 0)
        {
            if (_audioService == null || string.IsNullOrEmpty(audioUrl)) return;

            await _audioService.InitializeAsync(audioUrl);
            await _audioService.PlayAsync(positionMs);

            ReadTotal = (long)_audioService.CurrentDurationMillisecond;
            ReadPosition = (long)positionMs;
        }

        #endregion

        #region 章节切换操作 

        /// <summary>
        /// 跳转到指定章节并播放
        /// </summary>
        public async Task NavigateToChapterAsync(BookChapter chapter, long positionMs = 0)
        {
            if (chapter == null) return;

            CurrentChapter = chapter;

            if (CurrentBook.DurChapterIndex == CurrentChapter.Index)
            {
                positionMs = CurrentBook.DurChapterPos;
            }

            // 获取音频URL并播放
            var audioUrl = await GetBookContentAsync();
            await PlayChapterAsync(audioUrl, (int)positionMs);

        }

        /// <summary>
        /// 通过索引跳转到章节
        /// </summary>
        public async Task NavigateToChapterByIndexAsync(int index, int positionMs = 0)
        {
            if (CurrentChapters == null || CurrentChapters.Count == 0) return;

            var chapter = CurrentChapters.FirstOrDefault(c => c.Index == index);
            if (chapter != null)
            {
                await NavigateToChapterAsync(chapter, positionMs);
            }
        }

        /// <summary>
        /// 上一章
        /// </summary>
        public async Task PreviousChapterAsync()
        {
            if (CurrentChapters == null || CurrentChapters == null || CurrentChapters.Count == 0) return;
            var index = CurrentChapter.Index;
            index--;
            var nextChapter = CurrentChapters.FirstOrDefault(c => c.Index == index);
            if (nextChapter != null)
            {
                await NavigateToChapterAsync(nextChapter);
            }
        }

        /// <summary>
        /// 下一章
        /// </summary>
        public async Task NextChapterAsync()
        {
            if (CurrentChapters == null || CurrentChapters == null || CurrentChapters.Count == 0) return;
            var index = CurrentChapter.Index;
            index++;
            var nextChapter = CurrentChapters.FirstOrDefault(c => c.Index == index);
            if (nextChapter != null)
            {
                await NavigateToChapterAsync(nextChapter);
            }
        }

        /// <summary>
        /// 是否有上一章
        /// </summary>
        public bool HasPreviousChapter => CurrentChapter?.Index > 0;

        /// <summary>
        /// 是否有下一章
        /// </summary>
        public bool HasNextChapter => CurrentChapters != null && CurrentChapters?.Count > 0 && CurrentChapter.Index < CurrentChapters.Count - 1;

        #endregion

        public void Load()
        {
            try
            {

                var json = _eventProvider.GetResourceManager().GetString("bs");
                var list = JsonConvert.DeserializeObject<List<BookSource>>(json);
                if (list?.Count > 0)
                {
                    BookSources.AddRange(list);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveToShelfAsync()
        {
            if (CurrentBookSource != null)
            {
                await _appDb.BookSourceDao.InsertOrReplaceAsync(CurrentBookSource);
            }


            if (CurrentBook != null && this.CurrentChapter != null && this.CurrentChapters?.Count > 0)
            {
                CurrentBook.Origin = CurrentBookSource?.BookSourceUrl ?? "unknow";
                CurrentBook.SetCurrentChapter(this.CurrentChapter, CurrentChapters?.LastOrDefault(), CurrentChapter.Index, ReadPosition);
                await _appDb.BookDao.InsertOrReplaceAsync(CurrentBook);
            }

            Trace.WriteLine("[SaveToShelfAsync] ok");
        }

        public async Task<List<Book>> SearchAsync(string key, int page = 1, Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null)
        {
            CurrentBookSource.ThrowIfNull();

            var searchBooks = await WebBook.SearchBookAsync(CurrentBookSource, key, page, filter, shouldBreak);
            if (searchBooks?.Count > 0)
            {
                return searchBooks.Select(s => s.ToBook()).ToList();
            }
            return new List<Book>();
        }

        public async Task<List<Book>> PreciseSearchAsync(string key, int page = 1)
        {
            CurrentBookSource.ThrowIfNull();

            var searchBooks = await WebBook.PreciseSearchAsync(CurrentBookSource, key, page);
            if (searchBooks?.Count > 0)
            {
                return searchBooks.Select(s => s.ToBook()).ToList();
            }
            return new List<Book>();
        }

        public async Task<Book> UpdateBookInfoAsync(bool canRename = true)
        {
            CurrentBookSource.ThrowIfNull();
            CurrentBook.ThrowIfNull();
            var book2 = await WebBook.GetBookInfoAsync(CurrentBookSource, CurrentBook, canRename);
            //CurrentBook = book;
            return CurrentBook;
        }

        public async Task<List<BookChapter>> GetBookChapterListAsync(bool query = false, bool runPreUpdateJs = false,
            CancellationToken cancellationToken = default)
        {
            CurrentBookSource.ThrowIfNull();
            CurrentBook.ThrowIfNull();

            if (CurrentChapters?.Count > 0)
            {
                return CurrentChapters;
            }

            var result = await _appDb.BookChapterDao.GetListAsync(p => p.BookUrl == CurrentBook.BookUrl);

            if (result != null)
            {
                List<BookChapter> result2 = null;
                if (query)
                {
                    result2 = await WebBook.GetChapterListAsync(CurrentBookSource, CurrentBook, runPreUpdateJs, cancellationToken);

                    if (result.SequenceEqual(result2))
                    {
                        return result;
                    }
                }

                if (query && result2.Count > 0)
                {
                    await _appDb.BookChapterDao.InsertOrReplaceAllAsync(result2);
                    result = result2;
                }
            }

            return CurrentChapters = (result ?? new List<BookChapter>());
        }

        public async Task<string> GetBookContentAsync()
        {
            CurrentBookSource.ThrowIfNull();
            CurrentBook.ThrowIfNull();
            CurrentChapter.ThrowIfNull();

            return await WebBook.GetContentAsync(CurrentBookSource, CurrentBook, CurrentChapter);
        }

        public async Task SaveAllChapterAsync(List<BookChapter> list)
        {
            await _appDb.BookChapterDao.InsertAllAsync(list);
        }

        public async Task<List<Book>> GetBookshelfAsync()
        {
            return await _appDb.BookDao.GetAllAsync();
        }

        public async Task UpdateBookshelfAsync()
        {
            var books = await GetBookshelfAsync();
            foreach (var item in books)
            {
                var bookSource = await item.GetBookSourceAsync(this._appDb);
                var book2 = await WebBook.GetBookInfoAsync(bookSource, book: item, false);
                await _appDb.BookDao.InsertOrReplaceAsync(book2);
            }
        }

        /// <summary>
        /// 将书籍添加到书架
        /// </summary>
        public async Task AddToBookshelfAsync(Book book)
        {
            await _appDb.BookDao.InsertAsync(book);
        }

        /// <summary>
        /// 从书架移除书籍
        /// </summary>
        public async Task RemoveFromBookshelfAsync(Book book)
        {
            await _appDb.BookDao.DeleteAsync(book);
        }

        /// <summary>
        /// 检查书籍是否已在书架中
        /// </summary>
        public async Task<bool> IsBookInBookshelfAsync(string bookUrl)
        {
            return await _appDb.BookDao.HasAsync(bookUrl);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _progressTimer?.Dispose();

            if (_audioService != null)
            {
                _audioService.IsPlayingChanged -= OnIsPlayingChanged;
                _audioService.PlayFinished -= OnPlayFinished;
                _audioService.PlayFailed -= OnPlayFailed;
            }
        }
    }
}
