import { EMPLOYEE_STATUS } from '@/constants/employee-status'

export type EmployeeStatus = (typeof EMPLOYEE_STATUS)[keyof typeof EMPLOYEE_STATUS]
