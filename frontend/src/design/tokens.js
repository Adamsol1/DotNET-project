// Global design values, defines colors, spacing, shadows etc and should be used for design concistency and easy changes

export const tokens = {
    color: {
        bg:'#0b0f14', //background color behind everything
        surface: 'rgba(20,32,40,0.75)', //cards surface color
        surfaceBorder: 'rgba(160,220,255,0.25)',
        text: '#e8f0f7',
        textMuted: '#a9b8c4',
        primary: '#3db7d8',
        primaryHover: '#2fa5c6',
        accent: '#2cc9b2',
        danger: '#e65a5a',
    },
    radius: { md: 14, lg: 22, xl: 28, pill: 999 }, //rounded corner sizes
    space: { xs: 4, sm: 8, md: 12, lg: 16, xl: 24, xxl: 32 },
    shadow: { md: '0 12px 40px rgba(0,0,0,0.35)' }, //shadow for cards
    z: { bg: 0, scene: 1, hud: 2, panel: 3, choices: 4 }, //layer order for components
    };