"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { notesService } from "@/features/notes/notes.service";
import type { Note } from "@/features/notes/notes.types";

type NoteFormProps = {
  note?: Note;
};

const inputClasses =
  "rounded-lg border border-border bg-background px-3 py-2 text-sm outline-none transition-colors focus:border-accent focus:ring-2 focus:ring-accent/30";

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
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="note-title" className="text-sm font-medium">
          Title
        </label>
        <input
          id="note-title"
          type="text"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="e.g. Grocery list"
          required
          className={inputClasses}
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="note-content" className="text-sm font-medium">
          Content
        </label>
        <textarea
          id="note-content"
          value={content}
          onChange={(event) => setContent(event.target.value)}
          placeholder="Write your note here..."
          required
          rows={4}
          className={inputClasses}
        />
      </div>

      {error && (
        <p role="alert" className="text-sm text-danger">
          {error}
        </p>
      )}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-lg bg-accent px-4 py-2 text-sm font-medium text-accent-foreground transition active:scale-[0.98] disabled:opacity-50"
      >
        {isSubmitting ? "Saving..." : note ? "Save changes" : "Add note"}
      </button>
    </form>
  );
}
