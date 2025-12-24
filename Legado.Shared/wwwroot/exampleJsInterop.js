// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

// 音频播放相关函数
export function playAudio(audioElement) {
  if (audioElement) {
    audioElement.play().catch(error => {
      console.error('Error playing audio:', error);
    });
  }
}

export function pauseAudio(audioElement) {
  if (audioElement) {
    audioElement.pause();
  }
}

export function setAudioSrc(audioElement, src) {
  if (audioElement) {
    audioElement.src = src;
  }
}
