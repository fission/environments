FROM ruby:3.4-alpine

RUN apk update
RUN apk add --no-cache build-base

COPY . /app
WORKDIR /app
RUN bundle install

ENTRYPOINT ["ruby"]
CMD ["server.rb"]
