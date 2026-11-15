<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { RESOURCE_KEYS } from '@/constants/i18n'
import { tr } from '@/i18n'
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
    <form class="dialog" @submit.prevent="submit">
      <label class="dialog__field">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_FIELD_SURNAME) }}
        <input v-model="draft.surname" type="text" />
        <span v-if="fieldErrors.surname" class="dialog__field-error">
          {{ fieldErrors.surname }}
        </span>
      </label>
      <label class="dialog__field">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_FIELD_NAME) }}
        <input v-model="draft.name" type="text" />
        <span v-if="fieldErrors.name" class="dialog__field-error">{{ fieldErrors.name }}</span>
      </label>
      <label class="dialog__field">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_FIELD_PATRONYMIC) }}
        <input v-model="draft.patronymic" type="text" />
        <span v-if="fieldErrors.patronymic" class="dialog__field-error">
          {{ fieldErrors.patronymic }}
        </span>
      </label>
      <label class="dialog__field">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_POSITION) }}
        <input v-model="draft.position" type="text" />
        <span v-if="fieldErrors.position" class="dialog__field-error">
          {{ fieldErrors.position }}
        </span>
      </label>
      <label class="dialog__field">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_PHONE) }}
        <input v-model="draft.phone" type="text" />
        <span v-if="fieldErrors.phone" class="dialog__field-error">{{ fieldErrors.phone }}</span>
      </label>

      <p v-if="error" class="dialog__error">{{ error }}</p>

      <div class="dialog__actions">
        <button type="submit" class="dialog__button" :disabled="saving">
          {{ tr(RESOURCE_KEYS.EMPLOYEE_SAVE) }}
        </button>
        <button type="button" class="dialog__button" @click="emit('close')">
          {{ tr(RESOURCE_KEYS.EMPLOYEE_CANCEL) }}
        </button>
      </div>
    </form>
  </ModalDialog>
</template>

<style scoped src="./employee-form-dialog.css"></style>
