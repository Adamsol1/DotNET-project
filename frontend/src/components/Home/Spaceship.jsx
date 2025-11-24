import React from "react";
import { motion } from "framer-motion";

const Spaceship = () => {
  return (
    <motion.div
      className="absolute top-10 right-16 w-[450px] h-[450px] z-20"
      initial={{ x: 100, opacity: 0 }}
      animate={{ 
        x: 0,
        opacity: 1,
        y: [0, -15, 0],
        rotate: [0, 3, 0, -3, 0],
      }}
      transition={{
        x: { duration: 1, ease: "easeOut" },
        opacity: { duration: 1 },
        y: { duration: 4, repeat: Infinity, ease: "easeInOut" },
        rotate: { duration: 5, repeat: Infinity, ease: "easeInOut" },
      }}
    >
      <img
        src="/assets/backgrounds/spaceship.png"
        alt="Spaceship"
        className="w-full h-full object-contain"
        style={{
          filter: "drop-shadow(0 0 20px rgba(100, 200, 255, 0.5))",
        }}
      />
    </motion.div>
  );
};

export default Spaceship;