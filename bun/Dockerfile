ARG BUN_BASE_IMG

FROM oven/bun:${BUN_BASE_IMG}

ARG BUN_ENV
ENV BUN_ENV $BUN_ENV

RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

COPY package.json /usr/src/app/
RUN bun install
COPY server.ts /usr/src/app/server.ts
COPY tsconfig.json /usr/src/app/tsconfig.json

CMD [ "bun", "run", "server.ts" ]

EXPOSE 8888