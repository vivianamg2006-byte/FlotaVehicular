import { forwardRef, useEffect, useImperativeHandle, useRef } from "react";
import { FLEET_VEHICLES } from "./vehicles.jsx";

const FINAL_GAP = 14;
const MIN_GAP = 8;
const INITIAL_GAP = 70;
const MAX_SCALE = 1.35;
const EASING_EXPONENT = 2.2;
const LEADER_END = 0.78;
const SAFETY_EXTRA_MS = 1500;

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}

function ease(progress) {
  return 1 - Math.pow(1 - clamp(progress, 0, 1), EASING_EXPONENT);
}

const FleetScene = forwardRef(function FleetScene(_props, ref) {
  const containerRef = useRef(null);
  const itemRefs = useRef([]);
  const layoutRef = useRef({ initialPositions: [], finalPositions: [], scale: 1 });
  const progressRef = useRef(0);
  const rafRef = useRef(0);
  const watchdogRef = useRef(0);
  const pendingRef = useRef(null);
  const runIdRef = useRef(0);
  const mountedRef = useRef(false);

  function prefersReducedMotion() {
    return typeof window !== "undefined" && Boolean(window.matchMedia?.("(prefers-reduced-motion: reduce)").matches);
  }

  function calculateLayout() {
    const width = containerRef.current?.clientWidth ?? 0;
    if (!width) return null;

    const baseWidth = FLEET_VEHICLES.reduce((total, vehicle) => total + vehicle.w, 0);
    const totalBaseWidth = baseWidth + FINAL_GAP * (FLEET_VEHICLES.length - 1);
    const available = Math.max(width - 24, 1);
    const scale = clamp(available / totalBaseWidth, 0.32, MAX_SCALE);
    const gap = Math.max(MIN_GAP, FINAL_GAP * scale);
    const initialGap = Math.max(INITIAL_GAP * scale, MIN_GAP + 8);

    const finalPositions = [];
    let rightEdge = width - 12;
    for (const vehicle of FLEET_VEHICLES) {
      const vehicleWidth = vehicle.w * scale;
      finalPositions.push(rightEdge - vehicleWidth);
      rightEdge -= vehicleWidth + gap;
    }

    const initialPositions = [];
    let leftEdge = -FLEET_VEHICLES[0].w * scale - 16;
    FLEET_VEHICLES.forEach((vehicle, index) => {
      if (index > 0) leftEdge -= initialGap + vehicle.w * scale;
      initialPositions.push(leftEdge);
    });

    return { initialPositions, finalPositions, scale };
  }

  function apply(progress) {
    progressRef.current = progress;
    const layout = layoutRef.current;
    if (!layout.initialPositions.length) return;

    FLEET_VEHICLES.forEach((vehicle, index) => {
      const element = itemRefs.current[index];
      if (!element) return;

      const end = LEADER_END + (index / (FLEET_VEHICLES.length - 1)) * (1 - LEADER_END);
      const localProgress = clamp(progress / end, 0, 1);
      const eased = ease(localProgress);
      const from = layout.initialPositions[index];
      const to = layout.finalPositions[index];
      const x = from + (to - from) * eased;

      element.style.width = `${vehicle.w * layout.scale}px`;
      element.style.height = `${vehicle.h * layout.scale}px`;
      element.style.transform = `translate3d(${x}px, 0, 0)`;
    });
  }

  function syncLayout() {
    const layout = calculateLayout();
    if (!layout) return false;
    layoutRef.current = layout;
    apply(progressRef.current);
    return true;
  }

  function settle(id) {
    if (!pendingRef.current || pendingRef.current.id !== id) return;

    const { resolve } = pendingRef.current;
    pendingRef.current = null;

    if (rafRef.current) cancelAnimationFrame(rafRef.current);
    if (watchdogRef.current) clearTimeout(watchdogRef.current);
    rafRef.current = 0;
    watchdogRef.current = 0;
    resolve();
  }

  function start(durationMs = 2200) {
    if (pendingRef.current) settle(pendingRef.current.id);
    runIdRef.current += 1;
    const id = runIdRef.current;
    const hasLayout = syncLayout();

    if (!mountedRef.current || !hasLayout || prefersReducedMotion()) {
      if (prefersReducedMotion()) apply(1);
      return Promise.resolve();
    }

    const total = Math.max(durationMs, 1);
    const startedAt = performance.now();

    return new Promise((resolve) => {
      pendingRef.current = { id, resolve };
      watchdogRef.current = setTimeout(() => settle(id), total + SAFETY_EXTRA_MS);

      const tick = (now) => {
        if (!pendingRef.current || pendingRef.current.id !== id) return;

        if (prefersReducedMotion()) {
          apply(1);
          settle(id);
          return;
        }

        const progress = clamp((now - startedAt) / total, 0, 1);
        apply(progress);

        if (progress < 1) {
          rafRef.current = requestAnimationFrame(tick);
          return;
        }

        apply(1);
        settle(id);
      };

      rafRef.current = requestAnimationFrame(tick);
    });
  }

  useImperativeHandle(ref, () => ({ start }), []);

  useEffect(() => {
    mountedRef.current = true;
    syncLayout();
    apply(prefersReducedMotion() ? 1 : 0);

    const handleResize = () => {
      syncLayout();
      if (prefersReducedMotion()) apply(1);
    };

    window.addEventListener("resize", handleResize);

    return () => {
      mountedRef.current = false;
      window.removeEventListener("resize", handleResize);
      if (pendingRef.current) settle(pendingRef.current.id);
      if (rafRef.current) cancelAnimationFrame(rafRef.current);
      if (watchdogRef.current) clearTimeout(watchdogRef.current);
      rafRef.current = 0;
      watchdogRef.current = 0;
      pendingRef.current = null;
    };
  }, []);

  return (
    <div className="fleet-scene" ref={containerRef} aria-hidden="true">
      {FLEET_VEHICLES.map((vehicle, index) => {
        const Art = vehicle.Art;
        return (
          <div
            className="fleet-vehicle"
            key={vehicle.id}
            ref={(element) => {
              itemRefs.current[index] = element;
            }}
            style={{ width: vehicle.w, height: vehicle.h }}
          >
            <Art />
          </div>
        );
      })}
    </div>
  );
});

export default FleetScene;
