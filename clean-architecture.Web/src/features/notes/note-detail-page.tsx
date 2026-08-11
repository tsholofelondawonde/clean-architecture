import { notFound } from "next/navigation";
import Link from "next/link";
import { ArrowLeftIcon } from "@heroicons/react/20/solid";
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
    <div className="flex w-full max-w-2xl flex-col gap-8 px-6 py-12">
      <Link
        href="/notes"
        className="inline-flex w-fit items-center gap-1.5 text-sm text-muted transition-colors hover:text-foreground"
      >
        <ArrowLeftIcon className="size-4" aria-hidden="true" />
        Back to notes
      </Link>
      <h1 className="text-2xl font-semibold tracking-tight">Edit note</h1>
      <NoteForm note={note} />
    </div>
  );
}
