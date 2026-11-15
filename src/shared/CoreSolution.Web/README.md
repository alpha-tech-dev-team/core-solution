# CoreSolution.Web

Общий код фронтендов `Atlas.Web` и `Pandora.Web`.

Внутри только модули без зависимостей от Vue, Pinia и роутера: их импортируют оба приложения
через алиас `@shared`.

## Как подключить

`tsconfig` приложения:

```json
"paths": { "@shared/*": ["../../../../src/shared/CoreSolution.Web/src/*"] }
```

`vite.config.ts` приложения:

```ts
'@shared': fileURLToPath(new URL('../../../../src/shared/CoreSolution.Web/src', import.meta.url))
```

Файлы с импортом `vue` сюда не кладём: TypeScript не найдёт пакет из этой папки.
Такие компоненты остаются в приложении.
