

export const HoloButton = ({onClick, disabled, children, active, color = "blue"}) => {

    const baseStyles = "relative group overflow-hidden font-mono uppercase tracking-widest transition-all duration-200 ease-out border-2";
    
    const colorStyles = {
        cyan: `border-cyan-500/50 text-cyan-400 hover:bg-cyan-500/10 hover:border-cyan-400 hover:shadow-[0_0_20px_rgba(34,211,238,0.4)]`,
        rose: `border-rose-500/50 text-rose-400 hover:bg-rose-500/10 hover:border-rose-400 hover:shadow-[0_0_20px_rgba(251,113,133,0.4)]`,
        emerald: `border-emerald-500/50 text-emerald-400 hover:bg-emerald-500/10 hover:border-emerald-400 hover:shadow-[0_0_20px_rgba(52,211,153,0.4)]`,
    };

    return (
        <button
            onClick={onClick}
            disabled={disabled}
            className={`
                ${baseStyles} 
                ${colorStyles[color]}
                ${disabled ? 'opacity-50 cursor-not-allowed grayscale' : 'cursor-pointer'}
                ${active ? 'bg-cyan-500/20 border-cyan-400 shadow-[0_0_15px_rgba(34,211,238,0.5)]' : 'bg-slate-900/80'}
                px-6 py-4 rounded-lg w-full
            `}
        >
            <div className="relative z-10 flex items-center justify-center gap-3">
                {children}
            </div>
            {/* Scanline effect overlay */}
            <div className="absolute inset-0 bg-gradient-to-b from-transparent via-white/5 to-transparent -translate-y-full group-hover:translate-y-full transition-transform duration-700 ease-in-out" />
        </button>
    );
    
}