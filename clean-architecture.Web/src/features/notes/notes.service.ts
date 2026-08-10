import { api } from "@/shared/lib/api";
import type { CreateNoteRequest, Note, UpdateNoteRequest } from "@/features/notes/notes.types";

export const notesService = {
  create: async (request: CreateNoteRequest): Promise<Note> => {
    const response = await api.post<Note>("/notes", request);
    return response.data;
  },

  update: async (id: string, request: UpdateNoteRequest): Promise<Note> => {
    const response = await api.put<Note>(`/notes/${id}`, request);
    return response.data;
  },

  remove: async (id: string): Promise<void> => {
    await api.delete(`/notes/${id}`);
  },
};
