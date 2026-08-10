import { notFound } from "next/navigation";
import Link from "next/link";
import { fetchServer } from "@/shared/lib/fetch-server";
import { NoteForm } from "@/features/notes/note-form";
import type { Note } from "@/features/notes/notes.types";

type NoteDetailPageProps = {
  id: string;
};

export async function NoteDetailPage({ id }: NoteDetailPageProps) {
  const note = await fetchServer<Note>(`/notes/${id}`);

  if (!note) {
    notFound();
  }

  return (
    <div className="flex w-full max-w-2xl flex-col gap-8 px-6 py-16">
      <Link href="/notes" className="text-sm text-zinc-600 hover:underline dark:text-zinc-400">
        &larr; Back to notes
      </Link>
      <h1 className="text-2xl font-semibold">Edit note</h1>
      <NoteForm note={note} />
    </div>
  );
}
