"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { notesService } from "@/features/notes/notes.service";
import type { Note } from "@/features/notes/notes.types";

type NoteListProps = {
  notes: Note[];
};

export function NoteList({ notes }: NoteListProps) {
  const router = useRouter();
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    setDeletingId(id);
    try {
      await notesService.remove(id);
      router.refresh();
    } finally {
      setDeletingId(null);
    }
  };

  if (notes.length === 0) {
    return <p className="text-zinc-600 dark:text-zinc-400">No notes yet.</p>;
  }

  return (
    <ul className="flex flex-col gap-2">
      {notes.map((note) => (
        <li
          key={note.id}
          className="flex items-center justify-between rounded border border-black/10 px-4 py-3 dark:border-white/20"
        >
          <Link href={`/notes/${note.id}`} className="font-medium hover:underline">
            {note.title}
          </Link>
          <button
            type="button"
            onClick={() => handleDelete(note.id)}
            disabled={deletingId === note.id}
            className="text-sm text-red-600 disabled:opacity-50"
          >
            {deletingId === note.id ? "Deleting…" : "Delete"}
          </button>
        </li>
      ))}
    </ul>
  );
}
