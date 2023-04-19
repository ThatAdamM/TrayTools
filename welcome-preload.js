const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('process', {
    startup: (dat) => ipcRenderer.send('welcomeStartup', dat),
    welcomeOnLoad: (data) => ipcRenderer.send("welcomeOnLoad", data)
})