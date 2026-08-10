export type Note = {
  id: string;
  title: string | null;
  content: string | null;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
};

export type CreateNoteRequest = {
  title: string;
  content: string;
};

export type UpdateNoteRequest = {
  title: string;
  content: string;
};
