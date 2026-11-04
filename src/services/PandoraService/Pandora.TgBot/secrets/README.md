# Локальные секреты

Файлы в этом каталоге не коммитятся (см. `.gitignore`).

## `bw_password`

Мастер-пароль Vaultwarden-аккаунта шлюза (`BW_LOGIN`). Создайте файл
`bw_password` — пароль в первой строке. Compose монтирует его в контейнер
`bw-gateway` как Docker secret `/run/secrets/bw_password`.
