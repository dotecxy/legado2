// audioPlayer.js - 原生音频播放器控制模块

export class AudioPlayer {
    constructor(audioElement, callback) {
        this.audioElement = audioElement;
        this.callback = callback;
        this.isInitialized = false;
        this.init();
    }

    init() {
        if (this.isInitialized) return;

        // 添加音频事件监听
        this.audioElement.addEventListener('play', () => {
            this.notifyStatus('playing', {
                currentTime: this.audioElement.currentTime,
                duration: this.audioElement.duration
            });
        });

        this.audioElement.addEventListener('pause', () => {
            this.notifyStatus('paused', {
                currentTime: this.audioElement.currentTime,
                duration: this.audioElement.duration
            });
        });

        this.audioElement.addEventListener('timeupdate', () => {
            this.notifyStatus('timeupdate', {
                currentTime: this.audioElement.currentTime,
                duration: this.audioElement.duration
            });
        });

        this.audioElement.addEventListener('ended', () => {
            this.notifyStatus('ended', {
                currentTime: this.audioElement.currentTime,
                duration: this.audioElement.duration
            });
        });

        this.audioElement.addEventListener('loadedmetadata', () => {
            this.notifyStatus('loadedmetadata', {
                currentTime: this.audioElement.currentTime,
                duration: this.audioElement.duration
            });
        });

        this.audioElement.addEventListener('error', (e) => {
            this.notifyStatus('error', {
                error: e.target.error,
                currentTime: this.audioElement.currentTime,
                duration: this.audioElement.duration
            });
        });

        this.isInitialized = true;
    }

    notifyStatus(event, data) {
        if (this.callback) {
            // 调用C#的回调方法
            this.callback.invokeMethodAsync('OnAudioStatus', event, data);
        }
    }

    // 播放控制方法
    async play() {
        try {
            await this.audioElement.play();
            return true;
        } catch (error) {
            console.error('Error playing audio:', error);
            this.notifyStatus('error', { error: error.message });
            return false;
        }
    }

    pause() {
        this.audioElement.pause();
    }

    setAudioSrc(src) {
        this.audioElement.src = src;
    }

    setVolume(volume) {
        // volume should be between 0 and 1
        this.audioElement.volume = Math.max(0, Math.min(1, volume));
    }

    setMuted(muted) {
        this.audioElement.muted = muted;
    }

    setPlaybackRate(rate) {
        // rate should be between 0.5 and 2.0
        this.audioElement.playbackRate = Math.max(0.5, Math.min(2.0, rate));
    }

    seekTo(time) {
        // time in seconds
        this.audioElement.currentTime = time;
    }

    getCurrentTime() {
        return this.audioElement.currentTime;
    }

    getDuration() {
        return this.audioElement.duration;
    }

    getPlaybackRate() {
        return this.audioElement.playbackRate;
    }

    getVolume() {
        return this.audioElement.volume;
    }

    getMuted() {
        return this.audioElement.muted;
    }

    getPaused() {
        return this.audioElement.paused;
    }

    // 销毁播放器实例，移除事件监听
    destroy() {
        this.audioElement.removeEventListener('play', this.handlePlay);
        this.audioElement.removeEventListener('pause', this.handlePause);
        this.audioElement.removeEventListener('timeupdate', this.handleTimeUpdate);
        this.audioElement.removeEventListener('ended', this.handleEnded);
        this.audioElement.removeEventListener('loadedmetadata', this.handleLoadedMetadata);
        this.audioElement.removeEventListener('error', this.handleError);
        this.isInitialized = false;
    }
}

// 导出工厂函数，用于创建AudioPlayer实例
export function createAudioPlayer(audioElement, callback) {
    return new AudioPlayer(audioElement, callback);
}
