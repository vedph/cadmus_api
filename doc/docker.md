# Docker

## Building a Docker Image

Currently we cannot rely on NuGet feeds, like MyGet or Google repository. Thus, the temporary workaround is consuming the required packages from a local folder in the build image. These packages are copied from the local NuGet repository of your machine using a batch.

Thus, to build an image follow these steps:

1. run `UpdateLocalPackages.bat` to update the local packages. Ensure that the version numbers are in synch with the versions used in your projects before running.
2. open a command prompt in the solution folder (where the `Dockerfile` is located) and run `docker build . -t naftis/fusi:cadmusapi`. If you forget to specify the tag, you can add it later with `docker tag <imageid> naftis/fusi:cadmusapi`.
3. push the image into your target repository: `docker push naftis/fusi:cadmusapi`.

## Consuming a Docker Image

### Windows - Setup Docker

- download and install from <https://hub.docker.com/editions/community/docker-ce-desktop-windows> (see <https://docs.docker.com/docker-for-windows/install/>).

Once installed, ensure you have switched Docker to Linux containers.

### Ubuntu - Setup Docker

In the consumer __Linux machine__, you must have installed *Docker* and *Docker compose*. To install (see <https://docs.docker.com/install/linux/docker-ce/ubuntu/>):

```bash
sudo apt-get update

sudo apt-get install \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg-agent \
    software-properties-common

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -

sudo add-apt-repository \
   "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
   $(lsb_release -cs) \
   stable"

sudo apt-get update

sudo apt-get install docker-ce docker-ce-cli containerd.io
```

You can verify that Docker is installed correctly by running the hello-world image:

```bash
sudo docker run hello-world
```

To automatically start Docker:

```bash
sudo systemctl start docker
```

and then:

```bash
sudo systemctl enable docker
```

Check for installation: `docker --version`.

### Ubuntu - Setup Docker-Compose

Install Docker compose:

```bash
sudo curl -L "https://github.com/docker/compose/releases/download/1.25.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
sudo ln -s /usr/local/bin/docker-compose /usr/bin/docker-compose
sudo curl -L https://raw.githubusercontent.com/docker/compose/1.25.0/contrib/completion/bash/docker-compose -o /etc/bash_completion.d/docker-compose
```

(replace `1.25.0` with the latest docker compose release). Test with `docker-compose --version`.

### Ubuntu - Other Software

Useful apps links for Ubuntu:

- [TeamViewer](https://www.teamviewer.com/en/download/linux/)
- [Chrome](https://www.google.com/intl/en-US/chrome/)
- [MongoDB Compass](https://www.mongodb.com/download-center?jmp=nav#compass)
- [VSCode](https://code.visualstudio.com/download)
- [NodeJS](https://www.digitalocean.com/community/tutorials/how-to-install-node-js-on-ubuntu-16-04):

```bash
curl -sL https://deb.nodesource.com/setup_12.x | sudo -E bash -
sudo apt-get install nodejs
```

Check node version: `node --version`, `npm --version`.

- [Angular CLI](https://tecadmin.net/install-angular-on-ubuntu/): uninstall and reinstall, or just install if no previous version:

```bash
sudo npm uninstall -g @angular/cli
sudo npm cache verify
sudo npm install -g @angular/cli@latest
```

### Consume Image

1. copy the `docker-compose.yml` file in the Linux machine.
2. login into your Docker repository: `sudo docker login --username <DOCKERUSERNAME>`: then insert your Linux username (sudo) password, and the Docker password.
3. open a terminal in the same folder of the Docker compose file just copied, and execute `sudo docker-compose up`.

To *connect to MongoDB databases* from the Linux Docker host, using e.g. Compass (<https://www.mongodb.com/download-center?jmp=nav#compass>):

- server: 127.0.0.1
- port: 27017
- no authentication
