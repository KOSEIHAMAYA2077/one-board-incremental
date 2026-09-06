mergeInto(LibraryManager.library, {
  MomentumWebQuit: function () {
    window.dispatchEvent(new Event('momentum-ended'));
  }
});
