import Link from 'next/link'

import { IconLink } from '@/components/IconLink'
import { Logo } from '@/components/Logo'
import AddIcon from './AddIcon'
import { BookIcon, GitHubIcon, ListIcon, TwitterIcon, MastodonIcon, FeedIcon } from './GeneralIcons';

export function Intro() {
  return (
    <>
      <div>
        <Link href="/" className="inline-flex items-center">
          <Logo className="inline-block h-11 w-auto" />
          <div className="text-2xl ml-3 font-semibold text-white font-display">orels Music System</div>
        </Link>
      </div>
      <h1 className="mt-14 font-display text-4xl/tight font-light text-white">
        Low-overhead Open-Source multi-zone <span className="bg-gradient-to-r from-lime-300 to-yellow-300 bg-clip-text tracking-tight text-transparent">Music System</span>
      </h1>
      <p className="mt-4 text-sm/6 text-gray-300">
        orels Music System is a very low-overhead music system you can use in your VRChat world. It is fast, incredibly configurable and easy-to-use.
      </p>
      <div className="my-4 flex items-center gap-2 justify-stretch grow">
        <a
          href="#installation"
          title="Get Started"
          className="relative text-white text-center font-sem grow isolate group rounded-md overflow-hidden px-4 py-2"
        >
          <span className="relative z-10">Get Started</span>
          <span className="absolute inset-0 rounded-md bg-gradient-to-b from-lime-300/80 to-yellow-300 opacity-50 transition-opacity group-hover:opacity-60" />
          <span className="absolute inset-0 rounded-md opacity-7.5 shadow-[inset_0_1px_1px_white] transition-opacity group-hover:opacity-10" />
        </a>
        <a
          href="vcc://vpm/addRepo?url=https://orels1.github.io/orels-Music-System/index.json"
          title="Add to VCC"
          className="relative text-white text-center grow isolate group rounded-md overflow-hidden px-4 py-2"
        >
          <span className="inline-flex">
            <span className="relative z-10">Add to VCC</span>
            <AddIcon className="ml-3" />
          </span>
          <span className="absolute inset-0 rounded-md bg-gradient-to-b from-white/80 to-white opacity-10 transition-opacity group-hover:opacity-15" />
          <span className="absolute inset-0 rounded-md opacity-7.5 shadow-[inset_0_1px_1px_white] transition-opacity group-hover:opacity-10" />
        </a>
      </div>
      <div className="mt-8 flex flex-wrap justify-center gap-x-1 gap-y-3 sm:gap-x-2 lg:justify-start">
        <IconLink href="/docs" icon={BookIcon} className="flex-none">
          Docs
        </IconLink>
        <IconLink href="#changelog" icon={ListIcon} className="flex-none">
          Changelog
        </IconLink>
        <IconLink href="https://github.com/orels1/orels-MusicSystem" icon={GitHubIcon} className="flex-none">
          GitHub
        </IconLink>
        <IconLink href="/feed.xml" icon={FeedIcon} className="flex-none">
          RSS
        </IconLink>
      </div>
    </>
  )
}

export function IntroFooter() {
  return (
    <p className="flex items-baseline gap-x-2 text-[0.8125rem]/6 text-gray-500">
      Developed with <span className="bg-clip-text text-transparent bg-gradient-to-tr from-amber-700 to-red-500 text-lg top-[1px] relative">❤️</span> by {' '}
      <IconLink href="https://twitter.com/orels1_" icon={TwitterIcon} compact large>
        orels1_
      </IconLink>
      <IconLink href="https://mastodon.gamedev.place/@orels1" icon={MastodonIcon} compact large>
        orels1
      </IconLink>
    </p>
  )
}
