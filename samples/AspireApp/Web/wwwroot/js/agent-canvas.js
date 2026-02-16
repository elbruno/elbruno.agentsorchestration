// Agent Canvas - HTML5 Canvas rendering engine for agent network visualization
// Two-ring layout: Orchestrator center, 6 core agents inner ring, 5 specialists outer ring
(function () {
    'use strict';

    const TWO_PI = Math.PI * 2;
    const INNER_RING_RATIO = 0.32;
    const OUTER_RING_RATIO = 0.46;
    const NODE_WIDTH = 120;
    const NODE_HEIGHT = 68;
    const NODE_RADIUS = 12;
    const ANIMATION_DURATION = 500;
    const GLOW_CYCLE = 2000;
    const PARTICLE_SPEED = 0.0015;

    let canvas, ctx, dotNetRef;
    let animFrameId = null;
    let resizeObserver = null;
    let themeObserver = null;
    let nodes = [];
    let connections = [];
    let centerX = 0, centerY = 0;
    let innerRadius = 140, outerRadius = 240;
    let startTime = performance.now();

    // ─── Theme palettes ───
    const DARK_PALETTE = {
        background: '#0d1117',
        nodeFill: '#161b22',
        nodeActiveFill: 'rgba(88,166,255,0.08)',
        border: '#30363d',
        borderComplete: '#3fb950',
        textIcon: '#ffffff',
        textIdle: '#484f58',
        textStatus: '#8b949e',
        textComplete: '#3fb950',
        ringInner: '#21262d',
        ringOuter: '#1b1f24',
        ringLabel: '#30363d',
        coreDot: '#58a6ff',
        specialistDot: '#6f42c1',
        completedConn: 'rgba(139,148,158,0.3)',
        phaseBadgeBg: 'rgba(88,166,255,0.15)',
        phaseBadgeBorder: 'rgba(88,166,255,0.4)',
        phaseBadgeText: '#58a6ff',
    };

    const LIGHT_PALETTE = {
        background: '#ffffff',
        nodeFill: '#f6f8fa',
        nodeActiveFill: 'rgba(9,105,218,0.06)',
        border: '#d0d7de',
        borderComplete: '#1a7f37',
        textIcon: '#24292f',
        textIdle: '#6e7781',
        textStatus: '#57606a',
        textComplete: '#1a7f37',
        ringInner: '#d8dee4',
        ringOuter: '#eaeef2',
        ringLabel: '#afb8c1',
        coreDot: '#0969da',
        specialistDot: '#8250df',
        completedConn: 'rgba(87,96,106,0.25)',
        phaseBadgeBg: 'rgba(9,105,218,0.1)',
        phaseBadgeBorder: 'rgba(9,105,218,0.35)',
        phaseBadgeText: '#0969da',
    };

    let palette = DARK_PALETTE;

    function detectTheme() {
        const attr = document.documentElement.getAttribute('data-theme');
        palette = (attr === 'light') ? LIGHT_PALETTE : DARK_PALETTE;
    }

    const CORE_AGENTS = ['Planner', 'Coder', 'BuildReviewer', 'Designer', 'Researcher', 'Fixer'];
    const SPECIALIST_AGENTS = ['SecurityExpert', 'TestingExpert', 'DocumentationExpert', 'SoftwareArchitect'];

    // ─── Node class ───
    class AgentNode {
        constructor(name, icon, color, status, instruction) {
            this.name = name;
            this.icon = icon;
            this.color = color;
            this.status = status || 'idle';
            this.instruction = instruction || '';
            this.x = 0;
            this.y = 0;
            this.glowPhase = Math.random() * TWO_PI;
            this.activatedAt = 0;
            this.phases = []; // track which phases activated this agent
        }
    }

    // ─── Connection class ───
    // States: idle, animating, active, fading, completed
    class Connection {
        constructor(from, to, color) {
            this.from = from;
            this.to = to;
            this.color = color || '#58a6ff';
            this.state = 'idle';
            this.animStart = 0;
            this.particlePos = 0;
            this.opacity = 0;
        }
    }

    // ─── Position calculation ───
    function calculatePositions() {
        if (!canvas) return;
        const w = canvas.width;
        const h = canvas.height;
        centerX = w / 2;
        centerY = h / 2;
        const minDim = Math.min(w, h);
        innerRadius = minDim * INNER_RING_RATIO;
        outerRadius = minDim * OUTER_RING_RATIO;

        for (const node of nodes) {
            if (node.name === 'Orchestrator') {
                node.x = centerX;
                node.y = centerY;
                continue;
            }
            const coreIdx = CORE_AGENTS.indexOf(node.name);
            if (coreIdx >= 0) {
                const angle = -Math.PI / 2 + (TWO_PI * coreIdx) / CORE_AGENTS.length;
                node.x = centerX + innerRadius * Math.cos(angle);
                node.y = centerY + innerRadius * Math.sin(angle);
            } else {
                const specIdx = SPECIALIST_AGENTS.indexOf(node.name);
                const idx = specIdx >= 0 ? specIdx : 0;
                const total = Math.max(SPECIALIST_AGENTS.length, 1);
                const angle = -Math.PI / 2 + (TWO_PI * idx) / total + (Math.PI / total);
                node.x = centerX + outerRadius * Math.cos(angle);
                node.y = centerY + outerRadius * Math.sin(angle);
            }
        }
    }

    // ─── Drawing helpers ───
    function drawRoundedRect(x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.lineTo(x + w - r, y);
        ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r);
        ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h);
        ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r);
        ctx.quadraticCurveTo(x, y, x + r, y);
        ctx.closePath();
    }

    function getBezierPoint(fromNode, toNode, t) {
        const mx = (fromNode.x + toNode.x) / 2;
        const my = (fromNode.y + toNode.y) / 2;
        const dx = toNode.x - fromNode.x;
        const dy = toNode.y - fromNode.y;
        const len = Math.sqrt(dx * dx + dy * dy) || 1;
        const offset = len * 0.12;
        const cx = mx + (-dy / len) * offset;
        const cy = my + (dx / len) * offset;

        const u = 1 - t;
        return {
            x: u * u * fromNode.x + 2 * u * t * cx + t * t * toNode.x,
            y: u * u * fromNode.y + 2 * u * t * cy + t * t * toNode.y
        };
    }

    function getNodeByName(name) {
        return nodes.find(n => n.name.toLowerCase() === name.toLowerCase());
    }

    // ─── Draw connection ───
    function drawConnectionLine(conn, now) {
        const fromNode = getNodeByName(conn.from);
        const toNode = getNodeByName(conn.to);
        if (!fromNode || !toNode) return;

        let drawFraction = 1;
        let alpha = 1;
        let useDash = false;

        if (conn.state === 'animating') {
            const elapsed = now - conn.animStart;
            drawFraction = Math.min(elapsed / ANIMATION_DURATION, 1);
            alpha = drawFraction;
            if (drawFraction >= 1) {
                conn.state = 'active';
            }
        } else if (conn.state === 'active') {
            alpha = 0.5 + 0.3 * Math.sin((now - startTime) / 400);
        } else if (conn.state === 'fading') {
            const elapsed = now - conn.animStart;
            alpha = Math.max(1 - elapsed / 600, 0);
            if (alpha <= 0) {
                conn.state = 'completed';
                return;
            }
        } else if (conn.state === 'completed') {
            // Persistent dotted line for completed connections
            alpha = 0.35;
            useDash = true;
        } else if (conn.state === 'idle') {
            return; // don't draw idle connections
        }

        const steps = Math.max(Math.floor(drawFraction * 40), 2);

        if (useDash) ctx.setLineDash([6, 4]);

        ctx.beginPath();
        const p0 = getBezierPoint(fromNode, toNode, 0);
        ctx.moveTo(p0.x, p0.y);
        for (let i = 1; i <= steps; i++) {
            const t = i / 40;
            if (t > drawFraction) break;
            const p = getBezierPoint(fromNode, toNode, t);
            ctx.lineTo(p.x, p.y);
        }
        ctx.strokeStyle = conn.state === 'completed' ? palette.completedConn : conn.color;
        ctx.globalAlpha = alpha;
        ctx.lineWidth = conn.state === 'active' ? 2.5 : 1.5;
        ctx.stroke();

        if (useDash) ctx.setLineDash([]);

        // Traveling particle for active connections
        if (conn.state === 'active') {
            conn.particlePos = (conn.particlePos + PARTICLE_SPEED * 16) % 1;
            const pp = getBezierPoint(fromNode, toNode, conn.particlePos);

            ctx.beginPath();
            ctx.arc(pp.x, pp.y, 4, 0, TWO_PI);
            ctx.fillStyle = conn.color;
            ctx.globalAlpha = 0.9;
            ctx.fill();

            // Particle glow
            ctx.beginPath();
            ctx.arc(pp.x, pp.y, 8, 0, TWO_PI);
            ctx.fillStyle = conn.color;
            ctx.globalAlpha = 0.2;
            ctx.fill();
        }

        ctx.globalAlpha = 1;
    }

    // ─── Draw node ───
    function drawNode(node, now) {
        const nx = node.x - NODE_WIDTH / 2;
        const ny = node.y - NODE_HEIGHT / 2;
        const isOrchestrator = node.name === 'Orchestrator';

        // Glow effect for active nodes
        if (node.status === 'active') {
            const glowIntensity = 0.4 + 0.4 * Math.sin((now + node.glowPhase) / (GLOW_CYCLE / TWO_PI));
            ctx.shadowColor = node.color;
            ctx.shadowBlur = 12 + 8 * glowIntensity;
        } else {
            ctx.shadowColor = 'transparent';
            ctx.shadowBlur = 0;
        }

        // Background fill
        drawRoundedRect(nx, ny, NODE_WIDTH, NODE_HEIGHT, NODE_RADIUS);
        ctx.fillStyle = node.status === 'active' ? palette.nodeActiveFill : palette.nodeFill;
        ctx.fill();

        // Border
        let borderColor = palette.border;
        let borderWidth = 1.5;
        if (node.status === 'active') {
            borderColor = node.color;
            borderWidth = 2.5;
        } else if (node.status === 'complete') {
            borderColor = palette.borderComplete;
            borderWidth = 2;
        }
        drawRoundedRect(nx, ny, NODE_WIDTH, NODE_HEIGHT, NODE_RADIUS);
        ctx.strokeStyle = borderColor;
        ctx.lineWidth = borderWidth;
        ctx.stroke();

        // Reset shadow
        ctx.shadowColor = 'transparent';
        ctx.shadowBlur = 0;

        // Icon
        ctx.font = '18px "Segoe UI Emoji", "Apple Color Emoji", sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = palette.textIcon;
        ctx.fillText(node.icon, node.x, node.y - 16);

        // Name
        ctx.font = 'bold 11px "Segoe UI", sans-serif';
        ctx.fillStyle = node.color;
        ctx.fillText(node.name, node.x, node.y + 4);

        // Status text
        if (node.status === 'active') {
            ctx.font = '9px "Segoe UI", sans-serif';
            ctx.fillStyle = palette.textStatus;
            const label = truncate(node.instruction, 18);
            ctx.fillText(label, node.x, node.y + 20);
        } else if (node.status === 'complete') {
            ctx.font = '10px "Segoe UI", sans-serif';
            ctx.fillStyle = palette.textComplete;
            ctx.fillText('\u2713 done', node.x, node.y + 20);
        } else {
            ctx.font = '9px "Segoe UI", sans-serif';
            ctx.fillStyle = palette.textIdle;
            ctx.fillText('idle', node.x, node.y + 20);
        }

        // Category ring indicator (small dot in top-right corner)
        const isSpecialist = SPECIALIST_AGENTS.includes(node.name);
        if (!isOrchestrator) {
            ctx.beginPath();
            ctx.arc(node.x + NODE_WIDTH / 2 - 8, ny + 8, 3, 0, TWO_PI);
            ctx.fillStyle = isSpecialist ? palette.specialistDot : palette.coreDot;
            ctx.globalAlpha = 0.6;
            ctx.fill();
            ctx.globalAlpha = 1;
        }

        // Phase badge — show which phases called this agent e.g. [2-6]
        if (node.phases.length > 0) {
            const min = Math.min(...node.phases);
            const max = Math.max(...node.phases);
            const badgeText = min === max ? `[${min}]` : `[${min}-${max}]`;

            ctx.font = 'bold 9px "Segoe UI", sans-serif';
            const tw = ctx.measureText(badgeText).width;
            const bw = tw + 8;
            const bh = 16;
            const bx = node.x + NODE_WIDTH / 2 - bw - 2;
            const by = node.y + NODE_HEIGHT / 2 - bh - 2;

            drawRoundedRect(bx, by, bw, bh, 4);
            ctx.fillStyle = palette.phaseBadgeBg;
            ctx.fill();
            ctx.strokeStyle = palette.phaseBadgeBorder;
            ctx.lineWidth = 1;
            ctx.stroke();

            ctx.fillStyle = palette.phaseBadgeText;
            ctx.textAlign = 'center';
            ctx.fillText(badgeText, bx + bw / 2, by + bh / 2);
        }
    }

    // ─── Draw ring guides ───
    function drawRings() {
        // Inner ring (subtle dashed)
        ctx.beginPath();
        ctx.arc(centerX, centerY, innerRadius, 0, TWO_PI);
        ctx.strokeStyle = palette.ringInner;
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 8]);
        ctx.stroke();
        ctx.setLineDash([]);

        // Outer ring (subtle dashed)
        ctx.beginPath();
        ctx.arc(centerX, centerY, outerRadius, 0, TWO_PI);
        ctx.strokeStyle = palette.ringOuter;
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 8]);
        ctx.stroke();
        ctx.setLineDash([]);

        // Ring labels
        ctx.font = '9px "Segoe UI", sans-serif';
        ctx.textAlign = 'left';
        ctx.fillStyle = palette.ringLabel;
        ctx.fillText('Core Agents', centerX + innerRadius - 30, centerY - innerRadius - 6);
        ctx.fillText('Specialists', centerX + outerRadius - 28, centerY - outerRadius - 6);
    }

    // ─── Main render loop ───
    function render() {
        if (!canvas || !ctx) return;
        const now = performance.now();

        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // Background
        ctx.fillStyle = palette.background;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Draw ring guides
        drawRings();

        // Draw connections (behind nodes)
        for (const conn of connections) {
            drawConnectionLine(conn, now);
        }

        // Draw nodes (on top)
        for (const node of nodes) {
            drawNode(node, now);
        }

        animFrameId = requestAnimationFrame(render);
    }

    // ─── Hit testing ───
    function handleClick(e) {
        if (!canvas) return;
        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;
        const mx = (e.clientX - rect.left) * scaleX;
        const my = (e.clientY - rect.top) * scaleY;

        for (const node of nodes) {
            const nx = node.x - NODE_WIDTH / 2;
            const ny = node.y - NODE_HEIGHT / 2;
            if (mx >= nx && mx <= nx + NODE_WIDTH && my >= ny && my <= ny + NODE_HEIGHT) {
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnCanvasNodeClicked', node.name);
                }
                break;
            }
        }
    }

    function handleResize() {
        if (!canvas || !canvas.parentElement) return;
        const container = canvas.parentElement;
        const displayWidth = container.clientWidth;
        const displayHeight = container.clientHeight;
        canvas.width = displayWidth;
        canvas.height = displayHeight;
        canvas.style.width = displayWidth + 'px';
        canvas.style.height = displayHeight + 'px';
        calculatePositions();
    }

    function truncate(text, max) {
        if (!text) return '';
        return text.length <= max ? text : text.substring(0, max) + '\u2026';
    }

    // ─── Public API ───
    window.agentCanvas = {
        init: function (canvasId, ref) {
            canvas = document.getElementById(canvasId);
            if (!canvas) {
                console.error('Agent canvas not found:', canvasId);
                return;
            }
            ctx = canvas.getContext('2d');
            dotNetRef = ref;

            // Detect current theme
            detectTheme();

            // Watch for theme changes via data-theme attribute
            themeObserver = new MutationObserver(function (mutations) {
                for (const m of mutations) {
                    if (m.type === 'attributes' && m.attributeName === 'data-theme') {
                        detectTheme();
                    }
                }
            });
            themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });

            canvas.addEventListener('click', handleClick);

            resizeObserver = new ResizeObserver(() => handleResize());
            if (canvas.parentElement) {
                resizeObserver.observe(canvas.parentElement);
            }

            handleResize();
            startTime = performance.now();
            if (animFrameId) cancelAnimationFrame(animFrameId);
            animFrameId = requestAnimationFrame(render);
        },

        setNodes: function (nodeData) {
            if (!nodeData) return;
            nodes = nodeData.map(n => new AgentNode(n.name, n.icon, n.color, n.status, n.instruction));
            calculatePositions();
        },

        activateNode: function (name, color) {
            const node = getNodeByName(name);
            if (node) {
                node.status = 'active';
                if (color) node.color = color;
                node.activatedAt = performance.now();
            }
        },

        completeNode: function (name) {
            const node = getNodeByName(name);
            if (node) {
                node.status = 'complete';
            }
        },

        resetNode: function (name) {
            const node = getNodeByName(name);
            if (node) {
                node.status = 'idle';
                node.instruction = '';
                node.phases = [];
            }
        },

        updateNodeInstruction: function (name, instruction) {
            const node = getNodeByName(name);
            if (node) {
                node.instruction = instruction || '';
            }
        },

        addNodePhase: function (name, phaseNumber) {
            const node = getNodeByName(name);
            if (node && !node.phases.includes(phaseNumber)) {
                node.phases.push(phaseNumber);
            }
        },

        setTheme: function (isDark) {
            palette = isDark ? DARK_PALETTE : LIGHT_PALETTE;
        },

        drawConnection: function (from, to, color, animated) {
            let conn = connections.find(c =>
                c.from.toLowerCase() === from.toLowerCase() &&
                c.to.toLowerCase() === to.toLowerCase());

            if (conn) {
                conn.color = color || conn.color;
                conn.state = animated ? 'animating' : 'active';
                conn.animStart = performance.now();
                conn.particlePos = 0;
            } else {
                conn = new Connection(from, to, color);
                conn.state = animated ? 'animating' : 'active';
                conn.animStart = performance.now();
                connections.push(conn);
            }
        },

        deactivateConnection: function (from, to) {
            const conn = connections.find(c =>
                c.from.toLowerCase() === from.toLowerCase() &&
                c.to.toLowerCase() === to.toLowerCase());
            if (conn) {
                conn.state = 'fading';
                conn.animStart = performance.now();
            }
        },

        deactivateAll: function () {
            for (const node of nodes) {
                if (node.status === 'active') {
                    node.status = 'idle';
                }
            }
            for (const conn of connections) {
                if (conn.state === 'active' || conn.state === 'animating') {
                    conn.state = 'fading';
                    conn.animStart = performance.now();
                }
            }
        },

        resetAll: function () {
            for (const node of nodes) {
                node.status = 'idle';
                node.instruction = '';
                node.phases = [];
            }
            connections = [];
        },

        resize: function () {
            handleResize();
        },

        dispose: function () {
            if (animFrameId) {
                cancelAnimationFrame(animFrameId);
                animFrameId = null;
            }
            if (resizeObserver) {
                resizeObserver.disconnect();
                resizeObserver = null;
            }
            if (themeObserver) {
                themeObserver.disconnect();
                themeObserver = null;
            }
            if (canvas) {
                canvas.removeEventListener('click', handleClick);
            }
            canvas = null;
            ctx = null;
            dotNetRef = null;
            nodes = [];
            connections = [];
        }
    };
})();
