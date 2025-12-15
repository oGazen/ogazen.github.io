##### 常用运行命令

```bat
docker run -d -it --name trend-radar
  -v ./config:/app/config:ro
  -v ./output:/app/output
  -e ENV_VARIABLE="VALUE"
  image:tag

# -d 后台运行
# -i 保持一个标准输入流(stdin)打开
# -t 分配一个伪终端
# -v 绑定(相对路劲)
# -e 设置环境变量
# image:tag 镜像和标签
```

```bash
# ubuntu 安装包
apt/apt-get update install xxx
# alpine 安装包
apk udpate add xxx
# 常用包
sudo(执行权限) 
wget(下载) 
curl(下载)
vim(bash修改文件) 
gedit(bash修改文件)
cat(打印文本2bash)
ca-certificates(证书)
```

##### 容器内使用Docker(高权限)

* 运行容器必须绑定 `docker run -v /var/run/docker.sock:/var/run/docker.sock ......`

```bash
##### Ubuntu系统适用 #####
# 运行以下命令，更新软件包索引并安装添加 Docker 仓库所需的前置软件包：
sudo apt update
sudo apt install apt-transport-https curl


# 使用以下命令下载并导入 Docker 官方的 GPG 密钥：
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg


# 将 Docker 的官方仓库添加到 Ubuntu 24.04 LTS 的软件源列表：
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null


# 刷新软件包列表，以便系统识别新添加的 Docker 仓库：
# 执行以下命令在 Ubuntu 24.04 LTS 上安装最新版本的 Docker，包括 Docker 引擎及其相关组件：
sudo apt update
sudo apt install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# docker-ce：Docker Engine。
# docker-ce-cli：用于与 Docker 守护进程通信的命令行工具。$
# containerd.io：管理容器生命周期的容器运行时环境。
# docker-buildx-plugin：增强镜像构建功能的 Docker 扩展工具，特别是在多平台构建方面。
# docker-compose-plugin：通过单个 YAML 文件管理多容器 Docker 应用的配置管理插件。


# 使用以下命令检查 Docker 的运行状态：
sudo systemctl is-active docker




##### alpine系统适用 #####
apk update
apk add docker
```

##### 一些国内镜像

```bash
##### apt 镜像源 #####
# 修改此文件
sudo gedit /etc/apt/sources.list

# 添加 apt 镜像源 阿里
deb https://mirrors.aliyun.com/ubuntu/ jammy main restricted universe multiverse
deb-src https://mirrors.aliyun.com/ubuntu/ jammy main restricted universe multiverse
 
deb https://mirrors.aliyun.com/ubuntu/ jammy-security main restricted universe multiverse
deb-src https://mirrors.aliyun.com/ubuntu/ jammy-security main restricted universe multiverse
 
deb https://mirrors.aliyun.com/ubuntu/ jammy-updates main restricted universe multiverse
deb-src https://mirrors.aliyun.com/ubuntu/ jammy-updates main restricted universe multiverse
 
deb https://mirrors.aliyun.com/ubuntu/ jammy-backports main restricted universe multiverse
deb-src https://mirrors.aliyun.com/ubuntu/ jammy-backports main restricted universe multiverse

# 修改完执行
sudo apt update
```

##### 参考链接

[Docker 的数据卷 Volume](https://www.cnblogs.com/deshell/p/19053103)
[Ubuntu 安装 Docker](https://www.sysgeek.cn/install-docker-ubuntu/)
[Ubuntu中配置APT镜像源](https://www.psvmc.cn/article/2025-07-17-ubuntu-apt.html)
