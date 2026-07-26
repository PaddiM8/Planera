# Planera
A simple ticket-based project management web application.

![preview](preview.png)

## Features

* Collaborative projects
* Tickets
  * Rich text editing
  * Priority
  * Assign to users
  * Set deadlines
  * Notes/comments
* Push notifications
  * Get notified when a deadline is coming up
* Progressive web app (at it to your phone's home screen)
* OIDC login

## Getting Started

### Docker Compose

1. Download the docker-compose.yml file into an empty directory:
    ```shell
    curl -o docker-compose.yml https://raw.githubusercontent.com/PaddiM8/Planera/refs/heads/main/docker-compose.yml
    ```

2. Modify `docker-compose.yml` and replace both instances of
   `http://localhost:2000` with your domain. Additionally,
   it is also possible configure email sending and OIDC here.

3. Start docker compose:
    ```shell
    docker-compose up -d
    ```

Planera should now be running and accessible at localhost:2000.

## Development

### Dependencies

* .NET 10
* Node
* NPM
* Aspire

### Getting Started

1. Install [Aspire](https://aspire.dev/get-started/install-cli/). You may also want to install an extension for your editor/IDE.
2. Run `aspire run`
