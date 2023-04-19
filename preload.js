const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('process', {
    connect: async () => ipcRenderer.invoke('connect')
})