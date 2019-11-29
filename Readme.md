# LWSwnS
Short for **L**ite **W**eb **S**erver **W**ith u**N**safe **S**hell.
## Web Server
It can handle simple web task modularly.
### Extend Feature
The Web Server can be extended with modules. 
Samples: 
[`SimpleBlogModule`](./LWSwnS/SimpleBlogModule) [`PowerShellModule`](./LWSwnS/PowerShellModule) [`MarkdownFileModule`](./LWSwnS/MarkdownFileModule) [`BinaryFileTransmission`](./LWSwnS/BinaryFileTransmission)
### How well it works?
I don't know. However, with all sample modules loaded, memory usage is almost 17 MB (debug configuration). When transfering binary file using `HttpResponseData.SendFile(ref StreamWriter, FileStream)`, server memory usage won't grow suddenly.
## Unsafe Shell
It can receieve and run commands from ShellClient.
### Why unsafe?
I cannot ensure its safety. (Password compromise, unsafe modules, etc...)
### Commands
All commands(almost) are provided by modules.
## Modules
### Using Modules
Initially, the server will not load any of the modules which are provided with the preject for safety purpose.
To load modules, use `Init-Module <path-to-module-file>` to start initialization and add it to allow list.
### Build Your Own Module
Please look into sample modules mentioned before.
## It's a lite server, but why 170 MB?
It contains a runtime required to run PowerShell commands within Server process, if you don't use PowerShell module, you can delete it anyway. After deleting PowerShell runtime, LWSwnS should only take up less than 20 MB.
## License
`The MIT License`