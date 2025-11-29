import React, { useMemo, useState, useCallback } from "react";
import { motion } from "framer-motion";

const Stars = () => {
  // Generate random stars for shimmer effect
  // useMemo ensures stars are only generated once on mount, preventing re-calculation on re-renders
  const stars = useMemo(() => {
    return Array.from({ length: 40 }, (_, i) => ({
      id: i,
      x: Math.random() * 100,
      y: Math.random() * 100,
      size: Math.random() * 2.5 + 1,
      duration: Math.random() * 3 + 2,
      delay: Math.random() * 3,
    }));
  }, []);

  // Generate initial shooting star IDs
  const shootingStarIds = useMemo(() => [1, 2], []);

  return (
    <div className="absolute inset-0 overflow-hidden">
      {/* Background Image */}
      <div 
        className="absolute inset-0 bg-cover bg-center bg-no-repeat"
        style={{
          backgroundImage: "url('/assets/bg/background.png')",
          filter: "brightness(0.9)",
        }}
      />
      
      {/* Overlay gradient for depth */}
      <div className="absolute inset-0 bg-gradient-to-b from-transparent via-transparent to-black/30" />
      
      {/* Shimmering Stars */}
      {stars.map((star) => (
        <motion.div
          key={star.id}
          className="absolute rounded-full bg-white"
          style={{
            left: `${star.x}%`,
            top: `${star.y}%`,
            width: `${star.size}px`,
            height: `${star.size}px`,
            boxShadow: `0 0 ${star.size * 2}px rgba(255, 255, 255, 0.8)`,
          }}
          animate={{
            opacity: [0.3, 1, 0.3],
            scale: [1, 1.3, 1],
          }}
          transition={{
            duration: star.duration,
            repeat: Infinity,
            ease: "easeInOut",
            delay: star.delay,
          }}
        />
      ))}
      
      {/* Shooting stars */}
      {shootingStarIds.map((i) => (
        <ShootingStar key={`shooting-${i}`} delay={i * 6} />
      ))}
    </div>
  );
};

const ShootingStar = React.memo(({ delay }) => {
  // Initialize path data with random values using lazy initializer
  // Lazy initializer ensures Math.random() only runs once on mount
  const [pathData, setPathData] = useState(() => ({
    initialTop: Math.random() * 40, 
    startTop: Math.random() * 40, 
    endTop: Math.random() * 40 + 50,
  }));

  // Callback to generate new random path after animation completes
  // useCallback prevents function re-creation on re-renders
  const handleAnimationComplete = useCallback(() => {
    setPathData({
      initialTop: Math.random() * 40,
      startTop: Math.random() * 40,
      endTop: Math.random() * 40 + 50,
    });
  }, []);

  return (
    <motion.div
      className="absolute w-2 h-2 bg-white rounded-full"
      style={{
        boxShadow: "0 0 30px 4px rgba(255, 255, 255, 0.9)",
      }}
      initial={{
        top: `${pathData.initialTop}%`,
        left: "-5%", 
      }}
      animate={{
        top: [`${pathData.startTop}%`, `${pathData.endTop}%`],
        left: ["-5%", "110%"], 
        opacity: [0, 1, 1, 0], 
      }}
      transition={{
        duration: 2.5,
        delay: delay, 
        repeat: Infinity, 
        repeatDelay: 10, 
        ease: "easeIn", 
      }}
      onAnimationComplete={handleAnimationComplete} // Generate new path when animation finishes
    >
      {/* Shooting star tail */}
      <div 
        className="absolute top-0 left-0 w-16 h-0.5 bg-gradient-to-r from-white to-transparent"
        style={{
          transform: "translateX(-100%)",
        }}
      />
    </motion.div>
  );
});

export default Stars;