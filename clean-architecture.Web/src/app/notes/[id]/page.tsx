import type { Metadata } from "next";
import { NoteDetailPage } from "@/features/notes/note-detail-page";

export const metadata: Metadata = {
  title: "Edit note",
};

type PageProps = {
  params: Promise<{ id: string }>;
};

export default async function Page({ params }: PageProps) {
  const { id } = await params;
  return <NoteDetailPage id={id} />;
}
