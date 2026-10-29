<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { tr, trRequestError } from '@/i18n'
import { RESOURCE_KEYS } from '@/i18n/keys'
import { ROUTES } from '@/router/constants'
import { validateCredentials } from './login/validate-credentials'
import { LOADING_PLACEHOLDER } from './login/loading'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const errorMessage = ref('')
const isSubmitting = ref(false)

async function submit() {
  errorMessage.value = ''

  const validationError = validateCredentials({
    username: username.value,
    password: password.value
  })
  if (validationError) {
    errorMessage.value = validationError
    return
  }

  isSubmitting.value = true
  const loginError = await authStore.login(username.value, password.value)
  isSubmitting.value = false

  if (loginError === null) {
    router.push(ROUTES.HOME)
    return
  }
  errorMessage.value = trRequestError(loginError)
}
</script>

<template>
  <div class="login">
    <form class="login__card" @submit.prevent="submit">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <label class="login__field">
        <span>{{ tr(RESOURCE_KEYS.FIELD_USERNAME) }}</span>
        <input v-model="username" type="text" autocomplete="username" />
      </label>
      <label class="login__field">
        <span>{{ tr(RESOURCE_KEYS.FIELD_PASSWORD) }}</span>
        <input v-model="password" type="password" autocomplete="current-password" />
      </label>
      <p v-if="errorMessage" class="login__error">{{ errorMessage }}</p>
      <button type="submit" :disabled="isSubmitting">
        {{ isSubmitting ? LOADING_PLACEHOLDER : tr(RESOURCE_KEYS.AUTH_LOGIN) }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.login {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: var(--space-4);
}

.login__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  width: 100%;
  max-width: var(--login-card-width);
  padding: var(--space-7) var(--space-6);
  background: var(--color-surface);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.login__card h1 {
  font-size: var(--font-size-title);
  text-align: center;
  margin-bottom: var(--space-1);
}

.login__field {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: var(--font-size-small);
  color: var(--color-text-secondary);
}

.login__field input {
  padding: var(--space-2) var(--space-3);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
  outline: none;
}

.login__field input:focus {
  border-color: var(--color-primary);
}

.login__error {
  color: var(--color-error);
  font-size: var(--font-size-small);
  text-align: center;
}

.login__card button {
  padding: var(--space-3);
  background: var(--color-primary);
  color: var(--color-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.login__card button:disabled {
  background: var(--color-primary-disabled);
  cursor: default;
}
</style>