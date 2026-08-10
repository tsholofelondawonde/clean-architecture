"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { notesService } from "@/features/notes/notes.service";
import type { Note } from "@/features/notes/notes.types";

type NoteFormProps = {
  note?: Note;
};

export function NoteForm({ note }: NoteFormProps) {
  const router = useRouter();
  const [title, setTitle] = useState(note?.title ?? "");
  const [content, setContent] = useState(note?.content ?? "");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      if (note) {
        await notesService.update(note.id, { title, content });
      } else {
        await notesService.create({ title, content });
        setTitle("");
        setContent("");
      }
      router.refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3">
      <input
        type="text"
        value={title}
        onChange={(event) => setTitle(event.target.value)}
        placeholder="Title"
        required
        className="rounded border border-black/10 px-3 py-2 dark:border-white/20"
      />
      <textarea
        value={content}
        onChange={(event) => setContent(event.target.value)}
        placeholder="Content"
        required
        rows={4}
        className="rounded border border-black/10 px-3 py-2 dark:border-white/20"
      />
      {error && <p className="text-sm text-red-600">{error}</p>}
      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded bg-foreground px-4 py-2 text-background disabled:opacity-50"
      >
        {note ? "Save changes" : "Add note"}
      </button>
    </form>
  );
}
