import { fetchServer } from "@/shared/lib/fetch-server";
import { NoteForm } from "@/features/notes/note-form";
import { NoteList } from "@/features/notes/note-list";
import type { Note } from "@/features/notes/notes.types";

export async function NotesPage() {
  const notes = (await fetchServer<Note[]>("/notes")) ?? [];

  return (
    <div className="flex w-full max-w-2xl flex-col gap-10 px-6 py-12">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Notes</h1>
        <p className="text-sm text-muted">Create and manage your notes.</p>
      </div>
      <NoteForm />
      <NoteList notes={notes} />
    </div>
  );
}
