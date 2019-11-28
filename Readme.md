# LWSwnS
Short for **L**ite **W**eb **S**erver **W**ith u**N**safe **S**hell.
## Web Server?
It can handle simple web task modularly.
### Extend Feature
The Web Server can be extended with modules. 
Samples: 
[`SimpleBlogModule`](./LWSwnS/SimpleBlogModule) [`PowerShellModule`](./LWSwnS/PowerShellModule) [`MarkdownFileModule`](./LWSwnS/MarkdownFileModule)
## Unsafe Shell?
It can receieve and run commands from ShellClient.
### Why unsafe?
I cannot ensure its safety. (Password compromise, unsafe modules, etc...)
### Commands?
All commands(almost) are provided from modules.
## License?
The MIT License