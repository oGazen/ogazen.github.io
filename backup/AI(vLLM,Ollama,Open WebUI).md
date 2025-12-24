> [!NOTE]
> 平台 WIN10 AMD64 (x64)
> 软件 Dcoker.Desktop
> 显卡 NVIDIA GeForce GTX 1660 SUPER (Driver Version: 560.94; CUDA Version: 12.6)

> [!NOTE]
> Ollama `https://ollama.com/`
> Open WebUI `https://docs.openwebui.com/`
> vLLM `https://docs.vllm.com.cn/`
> huggingface `https://huggingface.co/`
> modelscope `https://www.modelscope.cn/`

> [!NOTE]
> ubuntu 24.04
> python 3.12
> uv `https://docs.astral.ac.cn/`
> venv
> build-essential
> cudatoolkit `https://developer.nvidia.com/cuda-12-6-2-download-archive?target_os=Linux&target_arch=x86_64&Distribution=Ubuntu&target_version=24.04&target_type=deb_local`

###### Ollama + Open WebUI

```bash
docker run -d -p 3000:8080 --gpus all --add-host=host.docker.internal:host-gateway \
-v open-webui:/app/backend/data \
--name open-webui \
--restart always \
ghcr.io/open-webui/open-webui:cuda

# --gpus all 映射宿主显卡到容器
# 访问地址 localhost:3000
```

###### vLLM

1. 安装 uv  python环境  vllm [快速入门 - vLLM 文档](https://docs.vllm.com.cn/en/latest/getting_started/quickstart.html#prerequisites)

```bash
uv venv --python 3.12 --seed
source .venv/bin/activate
uv pip install vllm --torch-backend=auto
```

2. 安装 CUDA Toolkit [NVIDIA CUDA Toolkit]([https://](https://developer.nvidia.com/cuda-12-6-2-download-archive?target_os=Linux&target_arch=x86_64&Distribution=Ubuntu&target_version=24.04&target_type=deb_local))

```bash
wget https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2404/x86_64/cuda-ubuntu2404.pin
sudo mv cuda-ubuntu2404.pin /etc/apt/preferences.d/cuda-repository-pin-600
wget https://developer.download.nvidia.com/compute/cuda/12.6.2/local_installers/cuda-repo-ubuntu2404-12-6-local_12.6.2-560.35.03-1_amd64.deb
sudo dpkg -i cuda-repo-ubuntu2404-12-6-local_12.6.2-560.35.03-1_amd64.deb
sudo cp /var/cuda-repo-ubuntu2404-12-6-local/cuda-*-keyring.gpg /usr/share/keyrings/
sudo apt-get update
sudo apt-get -y install cuda-toolkit-12-6
```

3. 设置环境变量

```bash
export VLLM_USE_MODELSCOPE=True
export LD_LIBRARY_PATH=/usr/lcoal/cuda-12.6/lib64:/.venv/lib/python3.12/site-packages/nvidia/cuda_runtime/lib:/.venv/lib/python3.12/site-packages/torch/lib:
export CUDA_HOME=/usr/local/cuda-12.6
export PATH=/usr/local/cuda/bin:$PATH
```

4. 测试运行

```bash
# 下载测试文件并执行
curl -O https://github.com/vllm-project/vllm/blob/main/examples/offline_inference/basic/basic.py
python3 basic.py
```

5. 可能错误和影响

```bash
# vLLM 实例：
llm = LLM(model="facebook/opt-125m",gpu_memory_utilization=0.8)
# gpu_memory_utilization=0.9 显存不足错误
# Free memory on device (5.01/6.0 GiB) on startup is less than desired GPU memory utilization (0.9, 5.4 GiB). Decrease GPU memory utilization or reduce GPU memory used by other processes.
# max_model_len=4069 最大模型长度警告
# To serve at least one request with the models's max seq len (131072), (4.00 GiB KV cache is needed, which is larger than the available KV cache memory (0.65 GiB). Based on the available memory, the estimated maximum model length is 21440. Try increasing `gpu_memory_utilization` or decreasing `max_model_len` when initializing the engine
# swap_space=4 交换空间警告
# Possibly too large swap space. 4.00 GiB out of the 7.72 GiB total CPU memory is allocated for the swap space.

# xxx.so 无法找到
find / -name xxx.so
# 将输出查找到的路径(xxx/lib)写入环境变量即可
export LD_LIBRARY_PATH=path1:$LD_LIBRARY_PATH


# WSL2 警告
# Using 'pin_memory=False' as WSL is detected. This may slow down the performance.

# Compute Capability 显卡版本不支持错误，不影响，会回退支持的方式处理
# Cannot use FA version 2 is not supported due to FA2 is only supported on devices with compute capability >= 8

# uv 错误 trying to connect: invalid peer certificate: UnknownIssuer - Even with SSL_CERT_FILE mapped
uv --native-tls --allow-insecure-host <your url> <your uv command here>

# pip 加速
uv pip install -index=https://mirrors.aliyun.com/pypi/simple/ <your package name>
```

6. 参考链接

* [vllm-project/vllm: A high-throughput and memory-efficient inference and serving engine for LLMs](https://github.com/vllm-project/vllm)
* [Python pip 安装与使用 | 菜鸟教程](https://www.runoob.com/w3cnote/python-pip-install-usage.html)
* [error trying to connect: invalid peer certificate: UnknownIssuer - Even with SSL\_CERT\_FILE mapped · Issue #1819 · astral-sh/uv](https://github.com/astral-sh/uv/issues/1819)
* [uv 安装、国内镜像配置与项目初始化 - 教程 - ljbguanli - 博客园](https://www.cnblogs.com/ljbguanli/p/19357762)
* [解决CUDA环境配置中的\`libcudart.so\`缺失问题-百度开发者中心](https://developer.baidu.com/article/details/3264277)
* [Failed to find C compiler. Please specify via CC environment variable · Issue #2997 · vllm-project/vllm](https://github.com/vllm-project/vllm/issues/2997)
* [解决 Ubuntu 中 /usr/local/cuda 缺失问题及 CUDA Toolkit 12.8 的安装修复过程-CSDN博客](https://blog.csdn.net/gs80140/article/details/147896728)
* [Linux 环境变量](https://cn.linux-terminal.com/?p=8350)


