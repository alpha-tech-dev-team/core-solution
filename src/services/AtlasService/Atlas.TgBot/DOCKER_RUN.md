# Запуск Atlas TgBot через Docker

## Требования
- Docker Desktop (запущен)
- Git
- Свой бот у @BotFather → токен
- Свой chat_id у @userinfobot

## Запуск

```bash
git clone https://github.com/alpha-tech-dev-team/core-solution.git
cd core-solution

cp src/services/AtlasService/Atlas.TgBot/.env.example src/services/AtlasService/Atlas.TgBot/.env

# Отредактируй .env (важно: формат с индексом __0 !):
# Telegram__BotToken=токен_от_BotFather
# Access__AllowedChatIds__0=твой_chat_id

docker run -d --name atlas-tgbot --env-file src/services/AtlasService/Atlas.TgBot/.env atlas-tgbot
```

## Проверка
```bash
docker logs -f atlas-tgbot
# Жди "Application started" и "AccessService initialized. Allowed chat IDs: ..."
```

## Ошибки
| Ошибка | Решение |
|---|---|
| `Telegram:BotToken не задан` | Заполни `.env` правильно (Telegram__BotToken) |
| `нет доступа` / `Access check: DENIED` | Неверный формат chat_id. Используй `Access__AllowedChatIds__0=твой_chat_id` |
| `409 Conflict` | У каждого свой токен (свой бот у @BotFather) |
| Docker не запускается | Запусти Docker Desktop |

## Остановка
```bash
docker stop atlas-tgbot
```

## Пересборка после правок кода
```bash
docker build -t atlas-tgbot -f src/services/AtlasService/Atlas.TgBot/Dockerfile .
docker stop atlas-tgbot && docker rm atlas-tgbot
docker run -d --name atlas-tgbot --env-file src/services/AtlasService/Atlas.TgBot/.env atlas-tgbot
```