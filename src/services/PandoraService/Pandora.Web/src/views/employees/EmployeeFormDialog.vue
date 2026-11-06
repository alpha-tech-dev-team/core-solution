<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { tr } from '@/i18n'
import type { EmployeeInput } from '@/stores/models/employee-input'
import { validateEmployeeForm, type EmployeeFormField, type EmployeeFormErrors } from '@/validation/employee'

const props = defineProps<{
  open: boolean
  initial: EmployeeInput | null
  error: string
}>()

const emit = defineEmits<{
  submit: [input: EmployeeInput]
  close: []
}>()

const draft = reactive<Record<EmployeeFormField, string>>({ name: '', surname: '', patronymic: '', position: '', phone: '' })
const fieldErrors = ref<EmployeeFormErrors>({})

watch(
  () => props.open,
  (open) => {
    if (!open) return

    draft.name = props.initial?.name ?? ''
    draft.surname = props.initial?.surname ?? ''
    draft.patronymic = props.initial?.patronymic ?? ''
    draft.position = props.initial?.position ?? ''
    draft.phone = props.initial?.phone ?? ''
    fieldErrors.value = {}
  }
)

function submit() {
  const normalized: EmployeeInput = {
    name: draft.name.trim(),
    surname: draft.surname.trim(),
    patronymic: draft.patronymic.trim(),
    position: draft.position.trim(),
    phone: draft.phone.trim()
  }

  fieldErrors.value = validateEmployeeForm(normalized)
  if (Object.keys(fieldErrors.value).length > 0) return

  emit('submit', { ...normalized, patronymic: normalized.patronymic || null })
}
</script>

<template>
  <div v-if="open" class="dialog-overlay" @click.self="emit('close')">
    <form class="dialog" @submit.prevent="submit">
      <h2 class="dialog__title">
        {{ initial ? tr('employee_edit_title') : tr('employee_add') }}
      </h2>

      <label class="dialog__field">
        {{ tr('employee_field_surname') }}
        <input v-model="draft.surname" type="text" />
        <span v-if="fieldErrors.surname" class="dialog__field-error">{{ fieldErrors.surname }}</span>
      </label>
      <label class="dialog__field">
        {{ tr('employee_field_name') }}
        <input v-model="draft.name" type="text" />
        <span v-if="fieldErrors.name" class="dialog__field-error">{{ fieldErrors.name }}</span>
      </label>
      <label class="dialog__field">
        {{ tr('employee_field_patronymic') }}
        <input v-model="draft.patronymic" type="text" />
        <span v-if="fieldErrors.patronymic" class="dialog__field-error">{{ fieldErrors.patronymic }}</span>
      </label>
      <label class="dialog__field">
        {{ tr('employee_position') }}
        <input v-model="draft.position" type="text" />
        <span v-if="fieldErrors.position" class="dialog__field-error">{{ fieldErrors.position }}</span>
      </label>
      <label class="dialog__field">
        {{ tr('employee_phone') }}
        <input v-model="draft.phone" type="text" />
        <span v-if="fieldErrors.phone" class="dialog__field-error">{{ fieldErrors.phone }}</span>
      </label>

      <p v-if="error" class="dialog__error">{{ error }}</p>

      <div class="dialog__actions">
        <button type="submit" class="dialog__button">{{ tr('employee_save') }}</button>
        <button type="button" class="dialog__button" @click="emit('close')">{{ tr('employee_cancel') }}</button>
      </div>
    </form>
  </div>
</template>

<style scoped>
.dialog-overlay {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-overlay);
}

.dialog {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  width: 100%;
  max-width: var(--size-form);
  padding: var(--space-5);
  background: var(--color-surface);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius);
}

.dialog__title {
  font-size: var(--font-size-large);
}

.dialog__field {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: var(--font-size-small);
}

.dialog__field input {
  padding: var(--space-2);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.dialog__field-error {
  color: var(--color-error);
}

.dialog__error {
  color: var(--color-error);
  font-size: var(--font-size-small);
}

.dialog__actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-2);
}

.dialog__button {
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.dialog__button:hover {
  background: var(--color-bg);
}
</style>
