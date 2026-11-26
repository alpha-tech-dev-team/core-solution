<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { useTranslations } from '@shared/i18n/create-i18n'
import PaginationNav from '@shared/components/PaginationNav.vue'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'
import { ROOM_LIMITS } from '@/constants/validation'
import { useRoomsView } from './use-rooms-view'

const {
  rooms,
  page: currentPage,
  totalPages,
  isLoading,
  isSaving,
  isFormVisible,
  editingId,
  formName,
  formDescription,
  selectedItem,
  selectedId,
  errorMessage,
  changePage,
  openCreateForm,
  closeForm,
  selectRow,
  editSelected,
  deleteSelected,
  save,
} = useRoomsView()
const { tr } = useTranslations()
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="page__nav">
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.HOME }">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </RouterLink>
        <LogoutButton />
      </nav>
    </header>

    <main class="page__body">
      <div class="toolbar">
        <h2>{{ tr(RESOURCE_KEYS.ROOMS_TITLE) }}</h2>
        <div v-if="!isFormVisible" class="toolbar__group">
          <button class="btn btn--primary" @click="openCreateForm">
            {{ tr(RESOURCE_KEYS.ROOM_ADD) }}
          </button>
          <button class="btn btn--ghost" :disabled="selectedItem === null" @click="editSelected">
            {{ tr(RESOURCE_KEYS.ROOM_EDIT) }}
          </button>
          <button class="btn btn--danger" :disabled="selectedItem === null" @click="deleteSelected">
            {{ tr(RESOURCE_KEYS.ROOM_DELETE) }}
          </button>
        </div>
      </div>

      <form v-if="isFormVisible" class="card card--padded form" @submit.prevent="save">
        <h3>{{ editingId === null ? tr(RESOURCE_KEYS.ROOM_ADD) : tr(RESOURCE_KEYS.ROOM_EDIT) }}</h3>
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.ROOM_NAME) }}</span>
          <input
            v-model="formName"
            class="input"
            type="text"
            :maxlength="ROOM_LIMITS.NAME_MAX"
          />
        </label>
        <label class="field">
          <span class="field__label">{{ tr(RESOURCE_KEYS.ROOM_DESCRIPTION) }}</span>
          <input
            v-model="formDescription"
            class="input"
            type="text"
            :maxlength="ROOM_LIMITS.DESCRIPTION_MAX"
          />
        </label>

        <p v-if="errorMessage" class="alert alert--error">{{ errorMessage }}</p>

        <div class="form__actions">
          <button type="button" class="btn btn--ghost" @click="closeForm">
            {{ tr(RESOURCE_KEYS.ROOM_CANCEL) }}
          </button>
          <button type="submit" class="btn btn--primary" :disabled="isSaving">
            {{ isSaving ? LOADING_PLACEHOLDER : tr(RESOURCE_KEYS.ROOM_SAVE) }}
          </button>
        </div>
      </form>

      <p v-if="errorMessage && !isFormVisible" class="alert alert--error">{{ errorMessage }}</p>
      <p v-if="isLoading" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>

      <p v-else-if="rooms.length === 0" class="text-muted">
        {{ tr(RESOURCE_KEYS.ROOMS_EMPTY) }}
      </p>

      <table v-else class="table">
        <thead>
          <tr>
            <th>{{ tr(RESOURCE_KEYS.ROOM_NAME) }}</th>
            <th>{{ tr(RESOURCE_KEYS.ROOM_DESCRIPTION) }}</th>
            <th>{{ tr(RESOURCE_KEYS.ROOM_CREATED_AT) }}</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="room in rooms"
            :key="room.id"
            :class="{ 'table__row--selected': room.id === selectedId }"
            @click="selectRow(room)"
          >
            <td>{{ room.name }}</td>
            <td>{{ room.description ?? tr(RESOURCE_KEYS.ROOM_NO_DESCRIPTION) }}</td>
            <td class="numeric">{{ room.createdAt }}</td>
          </tr>
        </tbody>
      </table>

      <PaginationNav
        :page="currentPage"
        :total-pages="totalPages"
        :prev-label="tr(RESOURCE_KEYS.ROOMS_PAGE_PREV)"
        :next-label="tr(RESOURCE_KEYS.ROOMS_PAGE_NEXT)"
        :info-label="tr(RESOURCE_KEYS.ROOMS_PAGE_INFO, currentPage, totalPages)"
        @change="changePage($event)"
      />
    </main>
  </div>
</template>
