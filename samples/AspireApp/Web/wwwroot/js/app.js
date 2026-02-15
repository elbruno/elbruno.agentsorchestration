window.scrollToBottom = function (element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.themeManager = {
    getTheme: function () {
        return localStorage.getItem('theme') || 'system';
    },
    setTheme: function (theme) {
        localStorage.setItem('theme', theme);
        this.applyTheme(theme);
    },
    applyTheme: function (theme) {
        if (theme === 'system') {
            const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
            document.documentElement.setAttribute('data-theme', systemTheme);
        } else {
            document.documentElement.setAttribute('data-theme', theme);
        }
    },
    init: function () {
        const theme = this.getTheme();
        this.applyTheme(theme);
        
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            if (this.getTheme() === 'system') {
                this.applyTheme('system');
            }
        });
    }
};

// Initialize theme immediately to prevent flash
window.themeManager.init();

window.copyToClipboard = function (text) {
    navigator.clipboard.writeText(text).then(function () {
        console.log('Copied to clipboard: ' + text);
    }, function (err) {
        console.error('Could not copy text: ', err);
    });
};

window.downloadTextFile = function (fileName, content) {
    const blob = new Blob([content], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};