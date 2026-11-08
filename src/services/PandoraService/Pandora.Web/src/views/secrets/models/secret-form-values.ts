import { type SecretForm } from '@/validation/secrets'

export interface SecretFormValues extends SecretForm {
  password: string
  isEditing: boolean
}
