rm -f /etc/apt/sources.list.d/docker.sources

printf 'Types: deb\nURIs: https://download.docker.com/linux/ubuntu\nSuites: %s\nComponents: stable\nArchitectures: %s\nSigned-By: /etc/apt/keyrings/docker.asc\n' "$(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")" "$(dpkg --print-architecture)" > /etc/apt/sources.list.d/docker.sources

cat /etc/apt/sources.list.d/docker.sources

apt update

apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

docker --version && docker compose version && docker run --rm hello-world
