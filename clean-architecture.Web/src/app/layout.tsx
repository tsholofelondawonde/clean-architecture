import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "Clean Architecture",
    template: "%s | Clean Architecture",
  },
  description: "Frontend for the Clean Architecture sample application.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="flex min-h-full flex-col">
        <header className="border-b border-border">
          <div className="mx-auto flex w-full max-w-3xl items-center px-6 py-4">
            <span className="text-sm font-semibold tracking-tight">
              Clean Architecture
            </span>
          </div>
        </header>
        <main className="flex flex-1 flex-col items-center">{children}</main>
      </body>
    </html>
  );
}
