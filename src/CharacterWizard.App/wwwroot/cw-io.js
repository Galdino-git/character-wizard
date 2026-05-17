window.cwIo = {
    download(filename, content) {
        const blob = new Blob([content], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        // Delay revoke so the WebView2 finishes the save.
        setTimeout(() => URL.revokeObjectURL(url), 1500);
    }
};
