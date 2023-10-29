import { type Metadata } from 'next'
import { Inter } from 'next/font/google'
import localFont from 'next/font/local'
import clsx from 'clsx'

import { Providers } from '@/app/providers'

import '@/styles/tailwind.css'

const inter = Inter({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-inter',
})

const monaSans = localFont({
  src: '../fonts/Mona-Sans.var.woff2',
  display: 'swap',
  variable: '--font-mona-sans',
  weight: '200 900',
})

export const metadata: Metadata = {
  title: 'orels Music System - Low-overhead Open-Source multi-zone Music System',
  description:
    'orels Music System is a very low-overhead music system you can use in your VRChat world. It is fast, incredibly configurable and easy-to-use.',
  alternates: {
    types: {
      'application/rss+xml': `${process.env.NEXT_PUBLIC_SITE_URL}/feed.xml`,
    },
  },
  metadataBase: new URL('https://musicsystem.orels.sh'),
  openGraph: {
    type: 'website',
    locale: 'en_US',
    url: 'https://musicsystem.orels.sh',
    description: 'orels Music System is a very low-overhead music system you can use in your VRChat world. It is fast, incredibly configurable and easy-to-use.',
    title: 'orels Music System',
    siteName: 'orels Music System',
    images: '/splash.png',
  },
  twitter: {
    site: '@orels1_',
    card: 'summary_large_image',
    description: 'orels Music System is a very low-overhead music system you can use in your VRChat world. It is fast, incredibly configurable and easy-to-use.',
    title: 'orels Music System',
    images: '/splash.png',
  },
}
export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html
      lang="en"
      className={clsx('h-full antialiased', inter.variable, monaSans.variable)}
      suppressHydrationWarning
    >
      <body className="flex min-h-full flex-col bg-white dark:bg-gray-950">
        <Providers>{children}</Providers>
      </body>
    </html>
  )
}
