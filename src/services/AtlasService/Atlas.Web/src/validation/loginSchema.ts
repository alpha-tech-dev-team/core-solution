import { z } from 'zod'
import { LoginValidation } from '@/constants/validation.Constants'
import { Atlas } from '@/resources'

export const loginSchema = z.object({
  username: z
    .string()
    .trim()
    .min(LoginValidation.UsernameMinLength, Atlas.ru.auth.errorUsernameRequired)
    .max(LoginValidation.UsernameMaxLength, Atlas.ru.auth.errorUsernameTooLong),
  password: z
    .string()
    .min(LoginValidation.PasswordMinLength, Atlas.ru.auth.errorPasswordRequired)
    .max(LoginValidation.PasswordMaxLength, Atlas.ru.auth.errorPasswordTooLong),
})
