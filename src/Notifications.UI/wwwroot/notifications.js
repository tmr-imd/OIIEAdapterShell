// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function updateNotificationCounter(count, replayAnimation) {
  var countSpan = document.querySelector('.notifications-nav .count-badge');
  countSpan.textContent = count;
  
   // Resets the animationName to 'none' and then empty '' to restart the animation.
  if (replayAnimation == true)
  {
    var animSpan = document.querySelector('.notifications-nav .animation-restartable');
    animSpan.style.animationName = 'none';
    requestAnimationFrame(() => setTimeout(() => animSpan.style.animationName = '', 0));
  }

  return true;
}
