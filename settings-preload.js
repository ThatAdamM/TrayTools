const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('process', {
    openFile: (dat) => ipcRenderer.invoke('dialog:openFile', dat),
    getIcons: () => ipcRenderer.invoke('getIcons'),
    toBase64: (image) => ipcRenderer.invoke('toBase64', image),
    setIcon: (data) => ipcRenderer.send('setIcon', data),
    openJSON: (data) => ipcRenderer.send('openJSON'),
    remIcon: (title) => ipcRenderer.send('remIcon', title),
    refresh: () => ipcRenderer.send('refresh'),
    editIcon: (num, data) => ipcRenderer.send("editIcon", num, data)
})