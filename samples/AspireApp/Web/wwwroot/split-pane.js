window.initSplitPane = function (dotnetRef) {
    const handle = document.querySelector('.split-handle');
    if (!handle) return;

    const pane = handle.parentElement;
    let dragging = false;

    handle.addEventListener('pointerdown', function (e) {
        dragging = true;
        handle.classList.add('active');
        handle.setPointerCapture(e.pointerId);
        e.preventDefault();
    });

    handle.addEventListener('pointermove', function (e) {
        if (!dragging) return;
        const rect = pane.getBoundingClientRect();
        const offsetX = e.clientX - rect.left;
        const pct = Math.min(85, Math.max(15, (offsetX / rect.width) * 100));
        dotnetRef.invokeMethodAsync('OnSplitResize', pct);
    });

    handle.addEventListener('pointerup', function (e) {
        dragging = false;
        handle.classList.remove('active');
    });

    handle.addEventListener('lostpointercapture', function () {
        dragging = false;
        handle.classList.remove('active');
    });
};
