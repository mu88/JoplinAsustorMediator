# JoplinAsustorMediator
Mediates between Joplin and the WebDAV server running on an ASUSTOR NAS.

# Reason for this repo
I am using [Joplin](https://joplinapp.org/) for taking notes. After migrating from my cloud storage to my local NAS, I was not able to sync my notes ([see this issue](https://github.com/laurent22/joplin/issues/4716)) via WebDAV. The reason for this was that my NAS answers to requests for not existing files with `HTTP 403 Forbidden` instead of `HTTP 404 Not Found`. Since the ASUSTOR support was either unwilling or unable to fix that bug in a timely manor, this little project was born out of necessity.

# How does it work
This ASP.NET Core Web API acts as a proxy for all Joplin requests. For this I've used the phantastic [AspNetCore.Proxy](https://github.com/twitchax/AspNetCore.Proxy) project. It offers everything I need for my little proxy.

Within Joplin, I configure my app as WebDAV synchronization target. Every request to my app from Joplin will be proxied towards my NAS and in case a `HTTP 403` occurs, it will be replaced with `HTTP 404` ([see here](https://github.com/mu88/JoplinAsustorMediator/blob/main/src/JoplinAsustorMediator/Controllers/ProxyController.cs#L32)). The response will be forwarded to Joplin which can act accordingly.

# Configuration
The app can be configured both via `appsettings.json` and environment variables, with the latter taking precedence.

There are three parameters within the `AppSettings` section:
- `JoplinUrl` → The URL to the ASUSTOR WebDAV endpoint, e. g. `https://192.168.178.21:9815`
- `CustomTlsValidation` → A boolean parameter that controls whether a custom TLS certificate validation shall be used or not. This is especially useful in a home network environment where self-signed certificates are used. If set to `true`, the `CertThumbprint` has to be specified.
- `CertThumbprint` → A string containing the SHA1 hash value of the self-signed TLS certificate provided by the `JoplinUrl` server, e. g. `de9f2c7fd25e1b3afad3e85a0bd17d9b100db4b3`.

If `CustomTlsValidation` is set to `true`, on every HTTPS request the `JoplinUrl` TLS certificate's thumbprint (SHA1 value) will be compared to the specified `CertThumbprint`. If they match, the TLS certificate validation will be considered successful.

# Deployment
This app runs within a Linux Docker container on my Raspberry Pi using nginx as reverse proxy. The following steps describe how to set everything up.

Since Joplin transfers the credentials via Basic Authentication, HTTPS should be used both for the communication between Joplin and the proxy and between the proxy and the ASUSTOR NAS. I've used [XCA](https://github.com/chris2511/xca) for managing the necessary keys.

## Install and run the container
- Pull the image from `https://hub.docker.com/r/mu88/joplinasustormediator` via `docker pull mu88/joplinasustormediator:latest`. It is build targetting the ARM32 platform.
- Run the container via `docker run --net=host -d -e AppSettings__JoplinUrl='https://192.168.178.21:9815' -e AppSettings__CustomTlsValidation='true' -e AppSettings__CertThumbprint='de9f2c7fd25e1b3afad3e85a0bd17d9b100db4b3' --name joplinasustormediator --restart always mu88/joplinasustormediator:latest`

Now you should be able to run the following commands:
- `curl http://localhost:5200/joplin` → Should return `I'm up and running`.

The previous command will enable custom TLS certificate validation. The underscore syntax is specific to access environment variables in ASP.NET Core ([see here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#environment-variables)).

One thing I really dislike is the usage of `--net=host`. This integrates the container's network into the host network. Therefore the typical Docker port exposures are not listed in the command, because they'd be useless: all the container ports will be accessible to the host anyway.  
I had to use this configuration because otherwise the connection to the `JoplinUrl` fails. There are plenty of articles covering that topic - just search for *Docker container access internet*.  
The biggest disadvantage for me is that I have to think about the deployment port `5200` at development time. With the regular Docker port exposures, usually that is not an issue.

## Configure nginx as reverse proxy
My configuration of `/etc/nginx/nginx.conf` looks like this:
```
server {
    listen 443;
    ssl on;
    ssl_certificate /etc/ssl/certs/raspberry.fritz.box.crt;
    ssl_certificate_key /etc/nginx/ssl/certificates/raspberry.fritz.box.pem;

    location /joplin {
        proxy_pass         http://localhost:5200/joplin;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection $connection_upgrade;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

This forwards all incoming requests like `https://raspberry.fritz.box/joplin/MyJoplinFolder/info.json` towards `http://localhost:5200/joplin/MyJoplinFolder/info.json` (the proxy app). Now the app forwards the traffic to `https://192.168.178.21:9815/MyJoplinFolder/info.json`.

## Configure Joplin
Within Joplin, the WebDAV URL used for synchronization now becomes `https://raspberry.fritz.box/joplin/MyJoplinFolder`.