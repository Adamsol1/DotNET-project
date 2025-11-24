import React from "react";
import { motion } from "framer-motion";

const Planet = () => {
  return (
    <motion.div
      className="absolute top-1/2 left-[5%] w-[400px] h-[400px] z-20"
      style={{ transform: "translateY(-50%)" }}
      initial={{ scale: 0, opacity: 0 }}
      animate={{ 
        scale: 1,
        opacity: 1,
        y: [0, -20, 0],
      }}
      transition={{
        scale: { duration: 1.5, ease: "easeOut" },
        opacity: { duration: 1.5 },
        y: { 
          duration: 4, 
          repeat: Infinity, 
          ease: "easeInOut",
          delay: 1.5
        },
      }}
    >
      {/* Planet image */}
      <img
        src="/assets/backgrounds/planet.png"
        alt="Planet"
        className="w-full h-full object-contain"
      />
    </motion.div>
  );
};

export default Planet;