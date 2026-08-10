import { fetchServer } from "@/shared/lib/fetch-server";
import { NoteForm } from "@/features/notes/note-form";
import { NoteList } from "@/features/notes/note-list";
import type { Note } from "@/features/notes/notes.types";

export async function NotesPage() {
  const notes = (await fetchServer<Note[]>("/notes")) ?? [];

  return (
    <div className="flex w-full max-w-2xl flex-col gap-8 px-6 py-16">
      <h1 className="text-2xl font-semibold">Notes</h1>
      <NoteForm />
      <NoteList notes={notes} />
    </div>
  );
}
