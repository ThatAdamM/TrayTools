async function go() {
const SSH = require("simple-ssh");
let ssh = new SSH({
    host: "localhost",
    user: "Adam",
    pass: 
})

await ssh.exec('dir', {
    out: function(stdout) {
        console.log(stdout);
        ssh.end()
    }
}).start();


}

go()
