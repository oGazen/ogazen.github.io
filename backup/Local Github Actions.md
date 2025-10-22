用于本地化测试处理Github流水线（Windows）

> 工具：vscode act docker.desktop

1. 安装vscode 扩展 [`Github Local Actions`](https://marketplace.visualstudio.com/items?itemName=SanjulaGanepola.github-local-actions)
   
   * [扩展地址源码](https://github.com/SanjulaGanepola/github-local-actions)
   * [扩展文档](https://sanjulaganepola.github.io/github-local-actions-docs/)
2. 安装最新 [act CLI](https://github.com/nektos/act/releases)
   
   * [nektos/act: 自构建源码](https://github.com/nektos/act)
   * 配置环境变量
3. 安装Docker.desktop [WSL 的手动安装步骤](https://learn.microsoft.com/zh-cn/windows/wsl/install-manual)
   
   * 启动CPU虚拟化，添加`虚拟机平台`
   * 添加 `使用于Linux的Windows子系统`
   * 安装`WSL2`
4. 启动Docker安装`catthehacker/ubuntu:act-24.04`镜像
5. 重启vscode可看到 `Github Local Action` 测边栏扩展已激活
6. Github根目录添加`.actrc`文件，写入

```
-P ubuntu-24.04=catthehacker/ubuntu:act-24.04
--pull=false
```

7. 运行测试
8. 其他参考链接
   * [码农必备！本地调试神器act，GitHub Actions最佳拍档 - 技术栈](https://jishuzhan.net/article/1963967697509203969)
