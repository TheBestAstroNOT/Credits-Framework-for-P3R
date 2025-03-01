// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Credits Framework for P3R',
			social: {
				github: 'https://github.com/TheBestAstroNOT/Credits-Framework-for-P3R',
				discord: 'https://discord.gg/naoto'
			},
			sidebar: [
				{
					label: 'Guides',
					autogenerate: { directory: 'guides' },
				}
			],
		}),
	],
});
