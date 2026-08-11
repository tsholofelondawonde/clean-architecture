"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { DocumentTextIcon, TrashIcon } from "@heroicons/react/24/outline";
import { notesService } from "@/features/notes/notes.service";
import type { Note } from "@/features/notes/notes.types";

type NoteListProps = {
  notes: Note[];
};

export function NoteList({ notes }: NoteListProps) {
  const router = useRouter();
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    setDeletingId(id);
    try {
      await notesService.remove(id);
      router.refresh();
    } finally {
      setDeletingId(null);
      setPendingDeleteId(null);
    }
  };

  if (notes.length === 0) {
    return (
      <div className="flex flex-col items-center gap-2 rounded-lg border border-dashed border-border px-6 py-12 text-center">
        <DocumentTextIcon className="size-8 text-muted" aria-hidden="true" />
        <p className="text-sm font-medium">No notes yet</p>
        <p className="text-sm text-muted">Add your first note using the form above.</p>
      </div>
    );
  }

  return (
    <ul className="flex flex-col gap-2">
      {notes.map((note) => {
        const isPendingDelete = pendingDeleteId === note.id;
        const isDeleting = deletingId === note.id;

        return (
          <li
            key={note.id}
            className="flex items-center justify-between gap-4 rounded-lg border border-border bg-surface px-4 py-3 transition-colors hover:border-foreground/20"
          >
            <Link
              href={`/notes/${note.id}`}
              className="min-w-0 flex-1 truncate font-medium hover:underline"
            >
              {note.title || "Untitled note"}
            </Link>

            {isPendingDelete ? (
              <div className="flex shrink-0 items-center gap-2">
                <span className="text-sm text-muted">Delete?</span>
                <button
                  type="button"
                  onClick={() => handleDelete(note.id)}
                  disabled={isDeleting}
                  className="rounded-md bg-danger px-2.5 py-1 text-sm font-medium text-white transition active:scale-[0.98] disabled:opacity-50"
                >
                  {isDeleting ? "Deleting..." : "Confirm"}
                </button>
                <button
                  type="button"
                  onClick={() => setPendingDeleteId(null)}
                  disabled={isDeleting}
                  className="rounded-md px-2.5 py-1 text-sm font-medium text-muted transition hover:text-foreground disabled:opacity-50"
                >
                  Cancel
                </button>
              </div>
            ) : (
              <button
                type="button"
                onClick={() => setPendingDeleteId(note.id)}
                className="shrink-0 rounded-md p-1.5 text-muted transition-colors hover:bg-danger/10 hover:text-danger"
                aria-label={`Delete "${note.title || "Untitled note"}"`}
              >
                <TrashIcon className="size-4" aria-hidden="true" />
              </button>
            )}
          </li>
        );
      })}
    </ul>
  );
}
