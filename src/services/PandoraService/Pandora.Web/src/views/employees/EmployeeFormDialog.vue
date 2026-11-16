<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import {
  validateEmployeeForm,
  type EmployeeFormErrors,
  type EmployeeFormField,
} from '@/validation/employee'
import type { EmployeeInput } from '@/models/employee-input'
import ModalDialog from '@/components/ModalDialog.vue'

const props = defineProps<{
  open: boolean
  initial: EmployeeInput | null
  error: string
  saving: boolean
}>()

const emit = defineEmits<{
  submit: [input: EmployeeInput]
  close: []
}>()

const { tr } = useTranslations()

const draft = reactive<Record<EmployeeFormField, string>>({
  name: '',
  surname: '',
  patronymic: '',
  position: '',
  phone: '',
})
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
  },
)

function submit(): void {
  const normalized: EmployeeInput = {
    name: draft.name.trim(),
    surname: draft.surname.trim(),
    patronymic: draft.patronymic.trim(),
    position: draft.position.trim(),
    phone: draft.phone.trim(),
  }

  fieldErrors.value = validateEmployeeForm(normalized)
  if (Object.keys(fieldErrors.value).length > 0) return

  emit('submit', { ...normalized, patronymic: normalized.patronymic || null })
}
</script>

<template>
  <ModalDialog
    :open="open"
    :title="initial ? tr(RESOURCE_KEYS.EMPLOYEE_EDIT_TITLE) : tr(RESOURCE_KEYS.EMPLOYEE_ADD)"
    @close="emit('close')"
  >
    <form class="form" @submit.prevent="submit">
      <label class="field">
        <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_FIELD_SURNAME) }}</span>
        <input v-model="draft.surname" class="input" type="text" />
        <span v-if="fieldErrors.surname" class="field__error">
          {{ tr(fieldErrors.surname) }}
        </span>
      </label>
      <label class="field">
        <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_FIELD_NAME) }}</span>
        <input v-model="draft.name" class="input" type="text" />
        <span v-if="fieldErrors.name" class="field__error">{{ tr(fieldErrors.name) }}</span>
      </label>
      <label class="field">
        <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_FIELD_PATRONYMIC) }}</span>
        <input v-model="draft.patronymic" class="input" type="text" />
        <span v-if="fieldErrors.patronymic" class="field__error">
          {{ tr(fieldErrors.patronymic) }}
        </span>
      </label>
      <label class="field">
        <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_POSITION) }}</span>
        <input v-model="draft.position" class="input" type="text" />
        <span v-if="fieldErrors.position" class="field__error">
          {{ tr(fieldErrors.position) }}
        </span>
      </label>
      <label class="field">
        <span class="field__label">{{ tr(RESOURCE_KEYS.EMPLOYEE_PHONE) }}</span>
        <input v-model="draft.phone" class="input" type="text" />
        <span v-if="fieldErrors.phone" class="field__error">{{ tr(fieldErrors.phone) }}</span>
      </label>

      <p v-if="error" class="alert alert--error">{{ error }}</p>

      <div class="form__actions">
        <button type="submit" class="btn btn--primary" :disabled="saving">
          {{ tr(RESOURCE_KEYS.EMPLOYEE_SAVE) }}
        </button>
        <button type="button" class="btn btn--ghost" @click="emit('close')">
          {{ tr(RESOURCE_KEYS.EMPLOYEE_CANCEL) }}
        </button>
      </div>
    </form>
  </ModalDialog>
</template>
